// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_GhostGroup
// *
// *    -------------------------------------------------------------------
// *    Hold ghost bricks to remove without causing lag
// *
// * ######################################################################

function ND_GhostGroup::deletionTick(%this)
{
	%max = $ND::GhostBricksPerTick;

	//For whatever reason, deleting many ghost bicks causes lag if many bricks are in the game
	if(getBrickCount() > 150000)
		%max /= 2;

	if(getBrickCount() > 300000)
		%max /= 2;

	if(getBrickCount() > 450000)
		%max /= 2;


	if(%this.getCount() <= %max)
	{
		%this.delete();
		return;
	}

	for(%i = 0; %i < %max; %i++)
		%this.getObject(0).delete();

	%this.schedule($ND::GhostBricksTickDelay, deletionTick);
}
