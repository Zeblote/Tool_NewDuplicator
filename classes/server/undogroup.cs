// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_UndoGroup
// *
// *    -------------------------------------------------------------------
// *    Handles undoing planting selections
// *
// * ######################################################################

//Start undo bricks
function SimSet::ndStartUndo(%this, %client)
{
	%this.brickCount = %this.getCount();

	if(!%this.brickCount)
	{
		%this.delete();
		return;
	}

	%client.ndUndoInProgress = true;
	%client.ndLastMessageTime = $Sim::Time;
	%this.ndTickUndo(%this.brickCount, %client);
}

//Tick undo bricks
function SimSet::ndTickUndo(%this, %count, %client)
{
	if(%count > %this.getCount())
		%start = %this.getCount();
	else
		%start = %count;

	if(%start > $Pref::Server::ND::ProcessPerTick)
		%end = %start - $Pref::Server::ND::ProcessPerTick;
	else
		%end = 0;

	for(%i = %start - 1; %i >= %end; %i--)
	{
		%brick = %this.getObject(%i);
		%brick.killBrick();
	}

	//If undo is taking long, tell the client how far we get
	if(%client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%client.ndLastMessageTime = $Sim::Time;

		%percent = mCeil(100 - (%end * 100 / %this.brickCount));
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo in progress...\n<font:Verdana:17>\c3" @ %percent @ "%\c6 finished.", 10);
	}

	if(%end <= 0)
	{
		%this.delete();
		%client.ndUndoInProgress = false;
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo finished.", 4);

		return;
	}
	
	%this.schedule(30, ndTickUndo, %end, %client);
}
