// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_HighlightSet
// *
// *    -------------------------------------------------------------------
// *    Handle highlighting and de-highlighting bricks
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
	if(!$NDH[%this, "Count"])
		return;

	deleteVariables("$NDH" @ %this @ "_*");
}

//Add a brick to the set and highlight it
function ND_HighlightSet::addBrick(%this, %brick)
{
	//If the brick isn't in a set already, highlight it
	if(!%brick.ndHighlightSet)
	{
		//Store current color to remove highlight later
		%brick.ndColor = %brick.getColorID();

		//If the brick already has the highlight color, apply color fx instead
		if(%brick.ndColor == $ND::BrickHighlightColor)
		{
			%brick.ndColorFx = %brick.getColorFxId();
			%brick.setColorFx($ND::BrickHighlightColorFx);
		}
		else
			%brick.setColor($ND::BrickHighlightColor);
	}

	%brick.ndHighlightSet = %this;

	//Add brick to highlight set
	%i = $NDH[%this, "Count"]++;
	$NDH[%this, %i - 1] = %brick;
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
	%end = $NDH[%this, "Count"];

	if(%end - %start > $ND::DeHighlightPerTick)
		%end = %start + $ND::DeHighlightPerTick;
	else
		%lastTick = true;

	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NDH[%this, %i];
		
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
		%this.deHighlightSchedule = %this.schedule($ND::DeHighlightTickDelay, removeHighlightTick, %end);
	}
	else
		%this.delete();
}
