// * ######################################################################
// *
// *    New Duplicator - Scripts - Server
// *    Highlight
// *
// *    -------------------------------------------------------------------
// *    Handles highlighting and de-highlighting groups of bricks
// *
// * ######################################################################

//Highlight group data $NDH::*
// $NDH::NextId : Next free id to use for new highlight group
// $NDH::Count : Total number of active highlight groups
//
// $NDHN[brick] : Number of groups a brick is in
// $NDHC[brick] : Original color of the brick
// $NDHF[brick] : Original color fx of the brick
//
// $NDH[group] : Count of bricks in a group
// $NDH[group, i] : Brick in group at position i

//Reserve a highlight group id
function ndNewHighlightGroup()
{
	//Increase group number
	$NDH::LastId++;
	$NDH::Count++;

	//Assign free id
	return $NDH::LastId;
}

//Remove highlight group and clean up garbage variables
function ndRemoveHighlightGroup(%group)
{
	//Lower group number
	$NDH::Count--;

	//Clear count to allow reuse of index
	$NDH[%group] = 0;

	//Cancel schedules
	cancel($NDHS[%group]);

	//If this is the most recently created group, pretend it never existed
	if($NDH::LastId == %group)
		$NDH::LastId--;

	//If this is the last highlight group, just delete ALL highlight variables
	if($NDH::Count < 1)
		deleteVariables("$NDH*");
}

//Add a brick to a highlight group
function ndHighlightBrick(%group, %brick)
{
	//If brick is not highlighted, do that
	if(!$NDHN[%brick])
	{
		$NDHF[%brick] = %brick.colorFxID;

		if($Pref::Server::ND::OldHighlightMethod)
		{
			$NDHC[%brick] = %brick.colorID;

			if($NDHC[%brick] != $ND::BrickHighlightColor)
			{
				%brick.setColor($ND::BrickHighlightColor);
				%brick.setColorFx(0);
			}
			else
				%brick.setColorFx(3);
		}
		else
			%brick.setColorFx(3);
	}

	//Increase group number of this brick
	$NDHN[%brick]++;

	//Add brick to highlight group
	$NDH[%group, ($NDH[%group]++) - 1] = %brick;
}

//Immediately start de-highlighting bricks
function ndStartDeHighlight(%group)
{
	cancel($NDHS[%group]);

	ndTickDeHighlight(%group, 0);
}

//De-highglight bricks after some time
function ndDeHighlightDelayed(%group, %delay)
{
	if(isEventPending($NDHS[%group]))
		return;

	$NDHS[%group] = schedule(%delay, 0, ndTickDeHighlight, %group, 0);
}

//Tick de-highlighting bricks
function ndTickDeHighlight(%group, %start)
{
	%end = $NDH[%group];

	if(%end - %start > $Pref::Server::ND::ProcessPerTick)
		%end = %start + $Pref::Server::ND::ProcessPerTick;
	else
		%lastTick = true;

	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NDH[%group, %i];

		if(isObject(%brick))
		{
			//If the brick is in no more groups, de-highlight it
			if(!($NDHN[%brick]--))
			{
				if($Pref::Server::ND::OldHighlightMethod)
					%brick.setColor($NDHC[%brick]);
					
				%brick.setColorFx($NDHF[%brick]);
			}
		}
	}

	if(!%lastTick)
		$NDHS[%group] = schedule(30, 0, ndTickDeHighlight, %group, %end);
	else
		ndRemoveHighlightGroup(%group);
}
