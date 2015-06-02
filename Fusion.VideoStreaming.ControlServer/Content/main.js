function EventHandler(CONFIG) { this.CONFIG = CONFIG; }
EventHandler.prototype.State = { left: false, right: false };
EventHandler.prototype.CursorPosition = { x: -1, y: -1 };
EventHandler.prototype.onContextMenu = function (event) {
	event.preventDefault();
};
EventHandler.prototype.onKeyDown = function (event) { console.log(event); };
EventHandler.prototype.onKeyUp = function (event) { };
EventHandler.prototype.onMouseEnter = function (event) {
	$(event.target)
		.on('contextmenu', this.onContextMenu.bind(this))
		.on('keydown', this.onKeyDown.bind(this))
		.on('keyup', this.onKeyUp.bind(this))
		.on('mousedown', this.onMouseDown.bind(this))
		.on('mouseleave', this.onMouseLeave.bind(this))
		.off('mouseenter')
	;
};
EventHandler.prototype.onMouseLeave = function (event) {
	$(event.target)
		.on('mouseenter', this.onMouseEnter.bind(this))
		.off('contextmenu')
		.off('keydown')
		.off('keyup')
		.off('mouseleave')
		.off('mousedown')
		.off('mouseup')
		.off('mousemove')
	;
};
EventHandler.prototype.onMouseDown = function (event) {
	if (event.button == 2) {
		this.State.right = true;
		console.log('right = ' + this.State.right);
	} else {
		this.State.left = true;
		console.log('left = ' + this.State.left);
	}
};
EventHandler.prototype.onMouseUp = function (event) {
	if (event.button == 2) {
		this.State.right = false;
		console.log('right = ' + this.State.right);
	} else {
		this.State.left = false;
		console.log('left = ' + this.State.left);
	}
};
EventHandler.prototype.onMouseMove = function (event) { };
EventHandler.prototype.report = function (eventType, key) {
	$.post({});
};

$.fn.attachEventHandler = function () {
	var Handler = new EventHandler();
	this.on('contextmenu', Handler.onContextMenu.bind(Handler))
		.on('keyup', Handler.onKeyUp.bind(Handler))
		.on('keyDown', Handler.onKeyDown.bind(Handler))
		.on('mouseup', Handler.onMouseUp.bind(Handler))
		.on('mousedown', Handler.onMouseDown.bind(Handler))
		.on('mousemove', Handler.onMouseMove.bind(Handler));
	return this;
};

jQuery(function () {
	$('#test').attachEventHandler();
});