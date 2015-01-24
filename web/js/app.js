var Player = {
	number: 0,
	id: 0,
	color: 'fuchsia'
};

var keysPressed = {};

//$(document).ready(function ()
//{
//	connectToServer();
//});

document.addEventListener("DOMContentLoaded", connectToServer, false);

function connectToServer()
{
	// Request a number / color
	$.ajax('/join', {
		dataType:'json',
		success: function(data)
		{
			// Bind player data
			Player.number = data.playerNumber;
			Player.id = data.id;
			Player.color = data.color;

			// Start polling for player updates
			window.setTimeout(checkForNewColor, 250);

			// Initiate party mode
			startGame();
		}
	});
}

var isPressed = {};
var locations = {};
var lastHeld = [];

function startGame()
{
	// Set background color and shit
	$('.control').css('background-color', Player.color);
	$('#bomb').text('Player ' + (Player.number + 1));

	document.body.addEventListener('pointerdown', handlePointer, true);
	document.body.addEventListener('pointermove', handlePointer, true);
	document.body.addEventListener('pointerup', handlePointer, true);

	// Also for keyboard
	$(document).on('keydown keyup', function(event)
	{
		event.preventDefault();
		handleKey(event);
	});

	$('.loading').hide();
}

function handlePointer(e)
{
	//console.log(e.type);
	//console.log(Date());
	if (e.type == 'pointerdown') {
		isPressed[e.pointerId] = true;
		locations[e.pointerId] = { x : e.clientX, y: e.clientY };
	}
	else if (e.type =='pointermove') {
		e.preventDefault();
		if (isPressed[e.pointerId]) {
			locations[e.pointerId] = { x : e.clientX, y: e.clientY };
		}
	}
	else if (e.type == 'pointerup') {
		isPressed[e.pointerId] = false;
		delete locations[e.pointerId];
	}
	updateMaybe();
	
	//return true;
}

function updateMaybe()
{
	var held = [];
	
	for (var i in isPressed)
	{
		if (isPressed[i]) 
		{
			var itemOn = testForHit(locations[i]);
			
			if (itemOn)
				held.push(itemOn);
		}
	}
	
	if (held.length != lastHeld.length)
	{
		lastHeld = held;
		tellServerWhatsUp(held);
	}
}




function testForHit(loc)
{
	var left = $('#left');
	
	var ctrls = {
		'left': $('#left'),
		'right': $('#right'),
		'up': $('#up'),
		'down': $('#down'),
		'bomb': $('#bomb')
	};
	
	//padding
	var x = loc.x - 20;
	var y = loc.y - 20;
	
	for (var i in ctrls) {
		var ele = ctrls[i];
		var p = ele.position();

		if (x >= p.left && x < p.left + ele.width()
			&& y >= p.top && y < p.top + ele.height())
		{
			return i;
		}
	}

	return null;
}

function handleKey(event)
{
	event.preventDefault();

	console.log('handling key');

	if (event.keyCode == 87) changedButton = 'up';
	else if (event.keyCode == 65) changedButton = 'left';
	else if (event.keyCode == 68) changedButton = 'right';
	else if (event.keyCode == 83) changedButton = 'down';
	else if (event.keyCode == 32) changedButton = 'bomb';
	else return;

	newState = event.type;

	if (newState == 'keydown') {
		keysPressed[changedButton] = 1;
	} else {
		delete keysPressed[changedButton]
	}

	tellServerWhatsUp(Object.keys(keysPressed));
}

function handleClick(event)
{
	event.preventDefault();

	console.log('handling click');

	changedButton = event.target.id;
	newState = event.type;

	if (newState == 'mousedown') {
		keysPressed[changedButton] = 1;
	} else {
		delete keysPressed[changedButton]
	}

	tellServerWhatsUp(Object.keys(keysPressed));
}

function tellServerWhatsUp(buttons)
{
	console.log(buttons);

	$.ajax('/update',
	{
		data: JSON.stringify({
			id: Player.id,
			controls: buttons
		}),
		type: 'POST',
		dataType: 'json',
		success: function(data)
		{
			// SHIT SON
			console.log('!!!!');
		}
	});
}

function checkForNewColor()
{
	$.ajax('/status',
	{
		data: JSON.stringify({
			id: Player.id
		}),
		type: 'POST',
		dataType: 'json',
		success: function(data)
		{
			// Set replacement color
			Player.color = data.color;

			$('.control').css('background-color', Player.color);
		},
		complete: function(data, status)
		{
			window.setTimeout(checkForNewColor, 250);
		} 
	});
}



// DEBUG AS FUCK
function logTouchStart() { console.log('touchStart'); }
function logTouchEnd() { console.log('touchEnd'); }
function logTouchCancel() { console.log('touchCancel'); }