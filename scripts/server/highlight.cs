// Handles highlighting and de-highlighting large groups of bricks.
// -------------------------------------------------------------------

//Highlight group data $NDH::*
// $NDH::LastId : The id of the last created highlight group
// $NDH::Count : Total number of active highlight groups
//
// $NDHN[brick] : Number of groups a brick is in
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

	//Set initial count
	$NDH[$NDH::LastId] = 0;

	//Assign free id
	return $NDH::LastId;
}

//Remove highlight group and clean up garbage variables
function ndRemoveHighlightGroup(%group)
{
	//Don't delete groups that don't exist
	if($NDH[%group] $= "")
		return;

	//Lower group number
	$NDH::Count--;

	//Clear count to allow reuse of index
	$NDH[%group] = "";

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
		%brick.setColorFx(3);
	}

	//Increase group number of this brick
	$NDHN[%brick]++;

	//Add brick to highlight group
	$NDH[%group, ($NDH[%group]++) - 1] = %brick;
}

//Start de-highlighting bricks
function ndStartDeHighlight(%group)
{
	//Don't do this if already de-highlighting
	%t = getTimeRemaining($NDHS[%group]);

	if(%t > 66 || %t == 0)
	{
		cancel($NDHS[%group]);
		ndTickDeHighlight(%group, 0);
	}
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

		//If the brick is in no more groups, de-highlight it
		if(isObject(%brick) && !($NDHN[%brick]--))
			%brick.setColorFx($NDHF[%brick]);
	}

	if(!%lastTick)
		$NDHS[%group] = schedule(30, 0, ndTickDeHighlight, %group, %end);
	else
		ndRemoveHighlightGroup(%group);
}
