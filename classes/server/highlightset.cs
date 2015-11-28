// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_HighlightSet
// *
// *    -------------------------------------------------------------------
// *    Handles highlighting and de-highlighting bricks
// *
// * ######################################################################

//Create highlight set
function ND_HighlightSet()
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_HighlightSet)
	);

	return %this;
}

//Destroy highlight set
function ND_HighlightSet::onRemove(%this)
{
	if(!$NH[%this, "Cnt"])
		return;

	deleteVariables("$NH" @ %this @ "_*");
}

//Add a brick to the set and highlight it
function ND_HighlightSet::addBrick(%this, %brick)
{
	//If the brick isn't in a set already, highlight it
	if(!%brick.ndHighlightSet)
	{
		%brick.ndColor = %brick.colorID;
		%brick.ndColorFx = %brick.colorFxID;

		//If the brick already has the highlight color, apply color fx instead
		if(%brick.ndColor == $ND::BrickHighlightColor)
			%brick.setColorFx(3);
		else
			%brick.setColor($ND::BrickHighlightColor);
	}

	%brick.ndHighlightSet = %this;

	//Add brick to highlight set
	%i = $NH[%this, "Cnt"]++;
	$NH[%this, %i - 1] = %brick;
}

//Immediately start de-highlighting bricks
function ND_HighlightSet::deHighlight(%this)
{
	%this.removeHighlightTick(0);
}

//De-highlight bricks after some time
function ND_HighlightSet::deHighlightDelayed(%this, %delay)
{
	%this.deHighlightSchedule = %this.schedule(%delay, removeHighlightTick, 0);
}

//Un-highlight some of the bricks (if finished, delete set)
function ND_HighlightSet::removeHighlightTick(%this, %start)
{
	%end = $NH[%this, "Cnt"];

	if(%end - %start > $Pref::Server::ND::ProcessPerTick)
		%end = %start + $Pref::Server::ND::ProcessPerTick;
	else
		%lastTick = true;

	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NH[%this, %i];
		
		//Only un-highlight this brick if it actually belongs in this set
		if(%brick.ndHighlightSet == %this)
		{
			//If the brick already had the highlight color, color fx must be removed
			if(%brick.ndColor == $ND::BrickHighlightColor)
				%brick.setColorFx(%brick.ndColorFx);
			else
				%brick.setColor(%brick.ndColor);

			%brick.ndHighlightSet = false;
		}
	}

	if(!%lastTick)
	{
		cancel(%this.deHighlightSchedule);
		%this.deHighlightSchedule = %this.schedule(30, removeHighlightTick, %end);
	}
	else
		%this.delete();
}
