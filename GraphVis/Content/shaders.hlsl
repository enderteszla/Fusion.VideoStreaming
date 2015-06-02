
#if 0
$ubershader INJECTION|SIMULATION|MOVE EULER|RUNGE_KUTTA|COLOR
$ubershader POINT|LINE
#endif

#define BLOCK_SIZE	512




struct PARTICLE3D {
	float3	Position; // 3 coordinates
	float3	Velocity;
	float4	Color0;
	float	Size0;
	float	TotalLifeTime;
	float	LifeTime;
	int		LinksPtr;
	int		LinksCount;
	float3	Acceleration;
	float	Mass;
	float	Charge;
	int		Id;
	int		Type;
	float	Assets;
	int		Links;



};

//struct 


struct LinkId {
	int id;
};


struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	int			MaxParticles;
	float		DeltaTime;
	float		LinkSize;
	float		MouseX;
	float		MouseY;
};


struct Link {
	int		par1;
	int		par2;
	float	length;
	float	force2;
	float3	orientation;
	float	weight;	
	int		linkType;
};



SamplerState						Sampler				: 	register(s0);
Texture2D							Texture 			: 	register(t0);

RWStructuredBuffer<PARTICLE3D>		particleBufferSrc	: 	register(u0);
StructuredBuffer<PARTICLE3D>		GSResourceBuffer	:	register(t1);

StructuredBuffer<LinkId>			linksPtrBuffer		:	register(t2);
StructuredBuffer<Link>				linksBuffer			:	register(t3);

cbuffer CB1 : register(c0) { 
	PARAMS Params; 
};

/*-----------------------------------------------------------------------------
	Simulation :
-----------------------------------------------------------------------------*/

#if defined(INJECTION) || defined(SIMULATION) || defined(MOVE)

struct BodyState
{
	float3 Position;
	float3 Velocity;
	float3 Acceleration;
	uint id;
};


struct Derivative
{
	float3 dxdt;
	float3 dvdt;
};



float3 SpringForce( in float3 bodyState, in float3 otherBodyState, float linkLength )
{
	float3 R			= otherBodyState - bodyState;			
	float Rabs			= length( R ) + 0.1f;
	float absForce		= 0.1f * ( Rabs - linkLength ) / ( Rabs );
	return mul( absForce, R*0.1 );
}


float3 RepulsionForce( in float3 bodyState, in float3 otherBodyState, float charge1, float charge2 )
{
	float3 R			= otherBodyState - bodyState;			
	float Rsquared		= R.x * R.x + R.y * R.y + R.z * R.z + 0.1f;
	float Rsixth		= Rsquared * Rsquared * Rsquared;
	float invRCubed		= - 10000.0f * charge1 * charge2  / sqrt( Rsixth );
	return mul( invRCubed, R );
}



float3 Acceleration( in PARTICLE3D prt, in int totalNum, in int particleId  )
{
	float3 acc = {0,0,0};
	float3 deltaForce = {0, 0, 0};
	float invMass = 1 / prt.Mass;
	
	PARTICLE3D other;
	[allow_uav_condition] for ( int lNum = 0; lNum < prt.LinksCount; ++ lNum ) {

		int otherId = linksBuffer[linksPtrBuffer[prt.LinksPtr + lNum].id].par1;

		if ( otherId == particleId ) {
			otherId = linksBuffer[linksPtrBuffer[prt.LinksPtr + lNum].id].par2;
		}

		other = particleBufferSrc[otherId];
		deltaForce += SpringForce( prt.Position, other.Position, linksBuffer[linksPtrBuffer[prt.LinksPtr + lNum].id].length );

	}

	
	[allow_uav_condition] for ( int i = 0; i < totalNum; ++i ) {
		other = particleBufferSrc[ i ];
		deltaForce += RepulsionForce( prt.Position, other.Position, prt.Charge, other.Charge );
	}

	acc += mul( deltaForce, invMass );
	acc -= mul ( prt.Velocity, 1.6f );

	return acc;
}




void IntegrateEUL_SHARED( inout BodyState state, in uint numParticles )
{
	
	state.Acceleration	= Acceleration( particleBufferSrc[state.id], numParticles, state.id );
}



[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int id = dispatchThreadID.x;

#ifdef INJECTION
	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[id];
		
//		if (p.LifeTime < p.TotalLifeTime) {
			
			particleBufferSrc[id] = p;
//		}
	}
#endif

#ifdef SIMULATION
	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[ id ];
		
	//	if (p.LifeTime < p.TotalLifeTime) {
	//		p.LifeTime += Params.DeltaTime;

			uint numParticles	=	0;
			uint stride			=	0;
			particleBufferSrc.GetDimensions( numParticles, stride );



			BodyState state;
			state.Position		=	p.Position;
			state.Velocity		=	p.Velocity;
			state.Acceleration	=	p.Acceleration;
			state.id			=	id;

#ifdef EULER

			IntegrateEUL_SHARED( state, Params.MaxParticles );

#endif
//			#ifdef COLOR
//
//			color = float4( 0.109f, 0.66f, 0.78f, 0 );
//
//#endif
#ifdef RUNGE_KUTTA
	
			IntegrateEUL_SHARED( state, Params.MaxParticles );

#endif

			//float color	= p.Size0;

			//float maxColor = 10.0f;
			//color = saturate( color / maxColor );

			//p.Color0	=	float4( color, - 0.5f * color +1.0f, - 0.5f * color +1.0f, 1 );
					

			p.Acceleration = state.Acceleration;

			particleBufferSrc[id] = p;
	//	}
	}
#endif
#ifdef MOVE
	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[ id ];

		

		p.Position.xyz += mul( p.Velocity, Params.DeltaTime );
		p.Velocity += mul( p.Acceleration, Params.DeltaTime );

		float4 RED		= float4(1, 0, 0, 1);
		float4 GREEN	= float4(0, 1, 0, 1);
		float4 BLUE		= float4(0, 0, 1, 1);
		float4 BLACK	= float4(0, 0, 0, 1);
		float4 WHITE	= float4(1, 1, 1, 0);
		float4 GRAY		= float4(1, 0.5f, 0.5f, 1);

		if (p.Type == 1){
			p.Color0 = GREEN;
			p.Size0 = 10;
			p.Size0 = 10 + p.Assets / 500;

			if (p.Size0 > 40) {
				p.Size0 = 40;
			}
		}

		else if (p.Type == 2) {
			p.Color0 = BLUE;

		}

		else if (p.Type == 3) {
			p.Color0 = WHITE;

		}

		else if (p.Type == 4) {
			p.Color0 = WHITE;
		}

		else if (p.Type == 5){
				p.Color0 = GRAY;
				p.Size0	 = 10.0f;
			}

		//else if (p.Type == 6){
		//		p.Color0 = float4(1, 0, 1, 1);
		//	}


		particleBufferSrc[ id ] = p;


	}
#endif


}

#endif




/*-----------------------------------------------------------------------------
	Rendering :
-----------------------------------------------------------------------------*/
/*

struct VSOutput {
	float4	Position		:	POSITION;
	float4	Color0			:	COLOR0;

	float	Size0			:	PSIZE;

	float	TotalLifeTime	:	TEXCOORD0;
	float	LifeTime		:	TEXCOORD1;
};*/

#if defined(POINT) || defined(LINE)

struct VSOutput {
int vertexID : TEXCOORD0;
};

struct GSOutput {
	float4	Position : SV_Position;
	float2	TexCoord : TEXCOORD0;
	float4	Color    : COLOR0;
};

/*
VSOutput VSMain( uint vertexID : SV_VertexID )
{
	PARTICLE prt = particleBufferSrc[ vertexID ];
	VSOutput output;

	output.Color0			=	prt.Color1;

	output.Size0			=	prt.Size0;
	
	output.TotalLifeTime	=	prt.TotalLifeTime;
	output.LifeTime			=	prt.LifeTime;

	output.Position			=	float4(prt.Position, 0, 1);

	return output;
}*/


VSOutput VSMain( uint vertexID : SV_VertexID )
{
	VSOutput output;
	output.vertexID = vertexID;
	return output;
}


float Ramp(float f_in, float f_out, float t) 
{
	float y = 1;
	t = saturate(t);
	
	float k_in	=	1 / f_in;
	float k_out	=	-1 / (1-f_out);
	float b_out =	-k_out;	
	
	if (t<f_in)  y = t * k_in;
	if (t>f_out) y = t * k_out + b_out;
	
	
	return y;
}


#ifdef POINT
[maxvertexcount(26)]
void GSMain( point VSOutput inputPoint[1], inout TriangleStream<GSOutput> outputStream )
{

	GSOutput p0, p1, p2, p3, p4, p5, p6;
	

	PARTICLE3D prt = GSResourceBuffer[ inputPoint[0].vertexID ];
	

	
		//float4 Color = prt.Color0;

		float factor = saturate(prt.LifeTime / prt.TotalLifeTime);

		float sz = prt.Size0 / 2;		

		float time = prt.LifeTime;



		float4 pos		=	float4( prt.Position.xyz, 1 );

		float4 posV		=	mul( pos, Params.View );

		float4 posScreen		=	mul( posV, Params.Projection );

		//if( abs( posScreen.x/posScreen.z - Params.MouseX ) < 0.02f && abs( posScreen.y/posScreen.z - Params.MouseY ) < 0.02f )
		//{
		//	sz *= 2;
		//}



		float R = 0.5f;
		float a = 60 * 0.01745329251;
		


		p0.Position = mul( posV + float4( sz, sz, 0, 0 ) , Params.Projection );
		p0.TexCoord = float2(0, 0);
		p0.Color = prt.Color0;

		p1.Position = mul( posV + float4( -sz, sz, 0, 0 ) , Params.Projection );
		p1.TexCoord = float2(1, 0);
		p1.Color = prt.Color0;

		p2.Position = mul( posV + float4( -sz, -sz, 0, 0 ) , Params.Projection );
		p2.TexCoord = float2(1, 1);
		p2.Color = prt.Color0;

		p3.Position = mul( posV + float4( sz, -sz, 0, 0 ) , Params.Projection );
		p3.TexCoord = float2(0, 1);
		p3.Color = prt.Color0;

		//p4.Position = mul( posV + float4( -sz, 0, 0, 0 ) , Params.Projection );
		//p4.TexCoord = float2(0, 0.5f);
		//p4.Color = prt.Color0;

		//p5.Position = mul( posV + float4( 0, 0, 0, 0 ) , Params.Projection );
		//p5.TexCoord = float2(0.5f, 0.5f);
		//p5.Color = prt.Color0;

		
		outputStream.Append(p0);
		outputStream.Append(p1);
		outputStream.Append(p2);
		outputStream.RestartStrip( );



		outputStream.Append(p0);
		outputStream.Append(p2);
		outputStream.Append(p3);
		outputStream.RestartStrip();



		//outputStream.Append(p2);
		//outputStream.Append(p3);
		//outputStream.Append(p5);
		//outputStream.RestartStrip();


		//outputStream.Append(p0);
		//outputStream.Append(p3);
		//outputStream.Append(p5);
		//outputStream.RestartStrip();


}


#endif

#ifdef LINE
[maxvertexcount(24)]
void GSMain( point VSOutput inputLine[1], inout TriangleStream<GSOutput> outputStream )
{
	GSOutput p1, p2, p3, p4, p5, p6, p7, p8;

	Link lk = linksBuffer[ inputLine[0].vertexID ];
	PARTICLE3D end1 = GSResourceBuffer[ lk.par1 ];
	PARTICLE3D end2 = GSResourceBuffer[ lk.par2 ];
	float4 pos1 = float4( end1.Position.xyz, 1 );
	//pos1.y = pos1.y - 0.2f;
	float4 pos2 = float4( end2.Position.xyz, 1 );
		//pos2.y = pos2.y - 0.2f;
	//float4 pos3 = float4( end1.Position.xyz, 1 );
	//	pos3.y = pos1.y + 0.2f;
	//float4 pos4 = float4( end2.Position.xyz, 1 );
	//	pos3.y = pos2.y + 0.2f;
	//float4 R		=	normalize(pos1 - pos2);
	float4 R		=	(pos1 - pos2);
	
	float Rxy2 = R.x*R.x + R.y*R.y;
	float Rxz2 = R.x*R.x + R.z*R.z;

	float4 Ort1 = mul( float4
		(
			sqrt( R.y*R.y/Rxy2 ),
			sqrt( R.x*R.x/Rxy2 ),
			0,
			0
		), 0.2f );

	float alpha = Ort1.x*R.z/(Ort1.y*R.x);
	float beta = 1+alpha*alpha;

	float4 Ort2 = float4(0, 0, 0, 0);
	Ort2.z = - sqrt( Ort1.x*Ort1.x / ( alpha*alpha*Ort1.y*Ort1.y + beta*Ort1.x*Ort1.x ) );
	Ort2.x = - sqrt( 1-beta*Ort2.z*Ort2.z );
	Ort2.y = alpha*Ort2.z;



	Ort2 = mul( Ort2, 0.2f );

	p1.Position = pos1 + Ort1 + Ort2;
	p2.Position = pos1 + Ort1 - Ort2;
	p3.Position = pos1 - Ort1 - Ort2;
	p4.Position = pos1 - Ort1 + Ort2;

	p5.Position = pos2 + Ort1 + Ort2;
	p6.Position = pos2 + Ort1 - Ort2;
	p7.Position = pos2 - Ort1 - Ort2;
	p8.Position = pos2 - Ort1 + Ort2;

	p1.Position = mul( mul(p1.Position, Params.View), Params.Projection );
	p2.Position = mul( mul(p2.Position, Params.View), Params.Projection );
	p3.Position = mul( mul(p3.Position, Params.View), Params.Projection );
	p4.Position = mul( mul(p4.Position, Params.View), Params.Projection );

	p5.Position = mul( mul(p5.Position, Params.View), Params.Projection );
	p6.Position = mul( mul(p6.Position, Params.View), Params.Projection );
	p7.Position = mul( mul(p7.Position, Params.View), Params.Projection );
	p8.Position = mul( mul(p8.Position, Params.View), Params.Projection );



//	float4 posV1	=	mul( pos1, Params.View );
//	float4 posV2	=	mul( pos2, Params.View );
	//float4 posV3	=	mul( pos3, Params.View );
	//float4 posV4	=	mul( pos4, Params.View );

//	float4 vect = normalize ( float4( ( mul(posV1.xy, 1/posV1.w) - mul(posV2.xy, 1/posV2.w ) ), 0, 0 ) );
//	float temp = vect.x;
//	vect.x = vect.y;
//	vect.y = -temp;

//	vect = mul( vect, 0.6f );
	//	PARTICLE3D end1 = GSResourceBuffer[inputLine[0].vertexID];
	//	float4 pos1 = float4( end1.Position.xyz, 1 );
	//	float4 pos2 = float4( end1.Position.xyz + float3(5,0,0), 1 );

	//	float4 posV1	=	mul( pos1, Params.View );
	//	float4 posV2	=	mul( pos2, Params.View );

	//p1.Position		=	mul( posV1 + vect, Params.Projection );
	//p2.Position		=	mul( posV1 - vect, Params.Projection );

	//p3.Position		=	mul( posV2 + vect, Params.Projection );
	//p4.Position		=	mul( posV2 - vect, Params.Projection );

	

	p1.TexCoord		=	float2(0, 0);
	p2.TexCoord		=	float2(0, 0);
	p3.TexCoord		=	float2(0, 0);
	p4.TexCoord		=	float2(0, 0);

	p5.TexCoord		=	float2(0, 0);
	p6.TexCoord		=	float2(0, 0);
	p7.TexCoord		=	float2(0, 0);
	p8.TexCoord		=	float2(0, 0);


	//p1.Position		=	mul( posV1, Params.Projection );
	//p2.Position		=	mul( posV2, Params.Projection );
	//p3.Position		=	mul( posV1 + float4( 0.5f, 0, 0, 0 ), Params.Projection );
	//p4.Position		=	mul( posV2 + float4( 0.5f, 0, 0, 0 ), Params.Projection );
	//p5.Position		=	mul( posV1 + float4( 0.5f, 0.5f, 0, 0 ), Params.Projection );
	//p6.Position		=	mul( posV2 + float4( 0.5f, 0.5f, 0, 0 ), Params.Projection );
	//p7.Position		=	mul( posV1 + float4( 0, 0.5f, 0, 0 ), Params.Projection );
	//p8.Position		=	mul( posV2 + float4( 0, 0.5f, 0, 0 ), Params.Projection );

	//p1.TexCoord		=	float2(0, 0);
	//p2.TexCoord		=	float2(0, 0);
	//p3.TexCoord		=	float2(0, 0);
	//p4.TexCoord		=	float2(0, 0);
	//p5.TexCoord		=	float2(0, 0);
	//p6.TexCoord		=	float2(0, 0);
	//p7.TexCoord		=	float2(0, 0);
	//p8.TexCoord		=	float2(0, 0);

	//p1.Color		=	end1.Color0;
	//p2.Color		=	end2.Color0;

	float4 color;

	float4 RED		= float4(1, 0, 0, 1);
	float4 GREEN	= float4(0, 1, 0, 1);
	float4 GREY		= float4(0.5f,0.5f, 0.5f, 0.3f);

	if (lk.linkType == 1){
		color = RED;	
	}
	//else if (end1.Position.z < 0 && end2.Position.z)
	//	color = GREEN;
	else
		color = GREY;

	p1.Color		=	color;
	p2.Color		=	color;
	p3.Color		=	color;
	p4.Color		=	color;
						
	p5.Color		=	color;
	p6.Color		=	color;
	p7.Color		=	color;
	p8.Color		=	color;

	/*p5.Color		=	float4(0.2f,0.2f, 0.2f, 0);
	p6.Color		=	float4(0.2f,0.2f, 0.2f, 0);
	p7.Color		=	float4(0.2f,0.2f, 0.2f, 0);
	p8.Color		=	float4(0.2f,0.2f, 0.2f, 0);*/
	//// if a bankrupt
	//if ( lk.linkType == 1){

	//	p1.Color		=	float4(1, 0, 0, 0);
	//	p2.Color		=	float4(1, 0, 0, 0);

	//}

	//else{

	//	if (lk.weight < 100){
	//	p1.Color		=	float4(0.1f, 0.1f, 0.1f, 0);
	//	p2.Color		=	float4(0.1f, 0.1f, 0.1f, 0);


	//}

	//else if (lk.weight > 100 && lk.weight <1000 ){
	//	p1.Color		=	float4(0.3f,0.3f, 0.3f, 0);
	//	p2.Color		=	float4(0.3f,0.3f, 0.3f, 0);

	//}

	//else if (lk.weight > 6000){
	//p1.Color		=	float4(0.6f, 0.6f, 0.6f, 0);
	//p2.Color		=	float4(0.6f, 0.6f, 0.6f, 0);

	//}

//}

	outputStream.Append(p1);
	outputStream.Append(p2);
	outputStream.Append(p3);
	outputStream.RestartStrip(); 

	outputStream.Append(p1);
	outputStream.Append(p3);
	outputStream.Append(p4);
	outputStream.RestartStrip();

	outputStream.Append(p2);
	outputStream.Append(p7);
	outputStream.Append(p3);
	outputStream.RestartStrip(); 

	outputStream.Append(p2);
	outputStream.Append(p6);
	outputStream.Append(p7);
	outputStream.RestartStrip();

	outputStream.Append(p3);
	outputStream.Append(p7);
	outputStream.Append(p8);
	outputStream.RestartStrip(); 

	outputStream.Append(p3);
	outputStream.Append(p8);
	outputStream.Append(p4);
	outputStream.RestartStrip(); 

	outputStream.Append(p1);
	outputStream.Append(p4);
	outputStream.Append(p5);
	outputStream.RestartStrip(); 

	outputStream.Append(p4);
	outputStream.Append(p8);
	outputStream.Append(p5);
	outputStream.RestartStrip(); 

	outputStream.Append(p1);
	outputStream.Append(p6);
	outputStream.Append(p2);
	outputStream.RestartStrip();

	outputStream.Append(p1);
	outputStream.Append(p5);
	outputStream.Append(p6);
	outputStream.RestartStrip(); 

	outputStream.Append(p5);
	outputStream.Append(p7);
	outputStream.Append(p6);
	outputStream.RestartStrip(); 

	outputStream.Append(p5);
	outputStream.Append(p8);
	outputStream.Append(p7);
	outputStream.RestartStrip(); 
}

#endif

#ifdef LINE
float4 PSMain( GSOutput input ) : SV_Target
{
	return input.Color;
}
#endif

#ifdef POINT

float4 PSMain( GSOutput input ) : SV_Target
{
	clip( input.Color.a < 0.1f ? -1:1 );
	return Texture.Sample( Sampler, input.TexCoord ) * input.Color;

}
#endif


#endif