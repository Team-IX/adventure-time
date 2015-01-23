// WHAT DO WE DO NOW?

var Player = {
	number: 0,
	id: 0,
	color: '#000000'
};

$(document).ready(function ()
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
	$('.control').on('mousedown mouseup', updateKeys);
});

function startGame()
{
	// Set background color and shit
	$('.bomb').css('control', Player.color);
	$('.bomb').text('Player ' + Player.playerNumber);

	// Set up event handlers
}

function updateKeys(event)
{
	console.log(event);
}
