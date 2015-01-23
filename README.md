# adventure-time
best game 2012 GOTY edition



API:

```
/join
returns: { playerNumber: 1, id: 1 }

^^ player number is 1-4
id is a number too, this is your unique id.



/update
{ id: 1, controls: [ 'down', 'bomb' ] }

controls is an array of:
left, up, down, right, bomb
contains whatever buttons are held down



var script = document.createElement("script");
script.src = "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js";
document.head.appendChild(script);


$.ajax('/update', { data:JSON.stringify({ id:1, controls: [ 'down', 'bomb' ]}), type:'POST', dataType:'json' });
```
