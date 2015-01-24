var Player = {
	number: 0,
	id: 0,
	color: 'fuchsia'
};

var keysPressed = {};

$(document).ready(function ()
{
	// Disable scrolling
	document.ontouchmove = function(event){
	    event.preventDefault();
	}

	connectToServer();
});

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

function startGame()
{
	// Set background color and shit
	$('.control').css({ background: Player.color });
	$('#bomb').text('Player ' + (Player.number + 1));

	// Set up event handlers for touch
	$('.control').on('mouseup mousedown', function(event) {
		event.preventDefault();
		handleClick(event);
	});

	controls = document.getElementsByClassName('control');

	for (var i = controls.length - 1; i >= 0; i--) {
		controls[i].addEventListener('touchstart', handleTouch, false);
		controls[i].addEventListener('touchend', handleTouch, false);
		controls[i].addEventListener('touchcancel', handleTouch, false);
	};

	// Also for keyboard
	$(document).on('keydown keyup', function(event)
	{
		event.preventDefault();
		handleKey(event);
	});

	$('.loading').hide();
}

function handleTouch(event)
{
	console.log(event);
	var touchedButtons = [];

	for (var i = event.touches.length - 1; i >= 0; i--) {
		touchedButtons.push(event.touches[i].target.id);
	};

	tellServerWhatsUp(touchedButtons);
}

function handleKey(event)
{
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
		data: Player.id,
		type: 'POST',
		dataType: 'json',
		success: function(data)
		{
			// Set replacement color
			Player.color = data.color;

			$('.control').css({ background: Player.color });

			window.setTimeout(checkForNewColor, 250);
		}
	});
}