// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_GhostGroup
// *
// *    -------------------------------------------------------------------
// *    Holds ghost bricks to remove without causing lag
// *
// * ######################################################################

//Create a new ghost group
function ND_GhostGroup()
{
	ND_ServerGroup.add(
		%this = new ScriptGroup(ND_GhostGroup)
	);

	return %this;
}

//Delete some of the bricks in this group
function ND_GhostGroup::tickDelete(%this)
{
	%max = $Pref::Server::ND::ProcessPerTick;
	%bricks = getBrickCount();

	//Deleting objects causes increasing lag with more bricks in total
	if(%bricks > 450000)
		%max /= 6;
	else if(%bricks > 300000)
		%max /= 4;
	else if(%bricks > 150000)
		%max /= 2;

	if(%this.getCount() <= %max)
	{
		%this.delete();
		return;
	}

	for(%i = 0; %i < %max; %i++)
		%this.getObject(0).delete();

	%this.schedule(30, tickDelete);
}
