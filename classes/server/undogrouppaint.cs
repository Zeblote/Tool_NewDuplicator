// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_UndoGroupPaint
// *
// *    -------------------------------------------------------------------
// *    Handles undoing painting selections
// *
// * ######################################################################

//Delete this undo group
function ND_UndoGroupPaint::onRemove(%this)
{
	if(%this.brickCount)
		deleteVariables("$NU" @ %this.client @ "_" @ %this @ "_*");
}

//Start undo paint
function ND_UndoGroupPaint::ndStartUndo(%this, %client)
{
	%client.ndUndoInProgress = true;
	%client.ndLastMessageTime = $Sim::Time;
	%this.ndTickUndo(%this.paintType, 0, %client);
}

//Tick undo paint
function ND_UndoGroupPaint::ndTickUndo(%this, %mode, %start, %client)
{
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NU[%client, %this, "B", %i];

		if(isObject(%brick))
		{
			%colorID = $NU[%client, %this, "V", %i];

			switch(%mode)
			{
				case 0:
					//Check whether brick is highlighted
					if($NDHN[%brick])
					{
						$NDHC[%brick] = %colorID;

						//Update color fx indicator
						if($NDHC[%brick] == $ND::BrickHighlightColor)
							%brick.setColorFx(3);
						else
							%brick.setColorFx(0);
					}
					else
						%brick.setColor(%colorID);

				case 1:
					//Check whether brick is highlighted
					if($NDHN[%brick])
						$NDHF[%brick] = %colorID;
					else
						%brick.setColorFx(%colorID);

				case 2:
					%brick.setShapeFx(%colorID);
			}
		}
	}

	//If undo is taking long, tell the client how far we get
	if(%client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%client.ndLastMessageTime = $Sim::Time;

		%percent = mFloor(%end * 100 / %this.brickCount);
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo in progress...\n<font:Verdana:17>\c3" @ %percent @ "%\c6 finished.", 10);
	}

	if(%end >= %this.brickcount)
	{
		%this.delete();
		%client.ndUndoInProgress = false;

		if(%start != 0)
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo finished.", 2);

		return;
	}
	
	%this.schedule(30, ndTickUndo, %mode, %end, %client);
}
