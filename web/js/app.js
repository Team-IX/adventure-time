// WHAT DO WE DO NOW?

var Player = {
	number: 0,
	id: 0,
	color: 'fuchsia'
};

var buttonsPressed = {
	up: 0,
	left: 0,
	right: 0,
	down: 0,
	bomb: 0
};

$(document).ready(function ()
{
	// initWithServer();
	initWithoutServer(); // Remove when actually running game
});

function initWithServer()
{
	// Request a number / color
	$.get({
		url: '/join',
		success: function(data)
		{
			// Bind player data
			Player.number = data.playerNumber;
			Player.id = data.id;
			Player.color = data.color;

			// Initiate party mode
			startGame();
		}
	});
}

function initWithoutServer()
{
	Player.number = 1;
	Player.id = 1;
	Player.color = 'fuchsia';

	$('.loading').hide();

	startGame();
}

function startGame()
{
	// Set background color and shit
	$('.control').css({ background: Player.color });
	$('#bomb').text('Player ' + Player.number);

	// Set up event handlers
	$('.control').on('mouseup mousedown', function(event) {
		event.preventDefault();
		updateKeys(event);
	});
}

function updateKeys(event)
{
	// Check which button changed state, and what action occurred
	changedButton = event.target.id;
	newState = event.type;

	// Update the buttonsPressed object
	if (newState == 'mousedown') {
		buttonsPressed[changedButton] = 1;
	} else {
		buttonsPressed[changedButton] = 0;
	}

	updateServer();
}

function updateServer()
{
	var buttonsToSend = [];
	var buttonKeys = Object.keys(buttonsPressed);

	// Generate list of buttons to send
	for (var i = buttonKeys.length - 1; i >= 0; i--) {
		console.log(buttonsPressed);
		if (buttonsPressed[buttonKeys[i]] == 1) {
			buttonsToSend.push(buttonKeys[i]);
		}
	};

	console.log(buttonsToSend);

	$.get({
		url: '/update',
		data: {
			id: Player.id,
			controls: buttonsToSend
		},
		success: function(data)
		{
			// SHIT SON
		}
	});
}