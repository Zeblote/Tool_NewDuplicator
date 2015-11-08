// * ######################################################################
// *
// *    New Duplicator - Server
// *    Handshake
// *
// *    -------------------------------------------------------------------
// *    Perform a handshake with the client-sided mod on someone joining
// *
// * ######################################################################

//Compare two version numbers (major.minor.patch)
function ND_CompareVersion(%ver1, %ver2)
{
	%ver1 = strReplace(%ver1, ".", " ");
	%ver2 = strReplace(%ver2, ".", " ");

	%count = getMax(getWordCount(%ver1), getWordCount(%ver2));

	for(%i = 0; %i < %count; %i ++)
	{
		%v1 = getWord(%ver1, %i);
		%v2 = getWord(%ver2, %i);

		if(%v1 > %v2)
			return 1;
		else if(%v1 < %v2)
			return 2;
	}

	return 0;
}

package NewDuplicator_Server
{
	function GameConnection::autoAdminCheck(%this)
	{
		%this.ndClient = false;
		%this.ndVersion = "0.0.0";
	
		//Send handshake to client
		commandToClient(%this, 'ndHandshake', $ND::Version);

		//Assume client doesn't have new duplicator if he doesn't answer in 5 seconds
		%this.ndHandshakeTimeout = %this.schedule(5000, "ndHandshakeTimeout");

		return parent::autoAdminCheck(%this);
	}
};

//Client probably doesn't have new duplicator installed
function GameConnection::ndHandshakeTimeout(%this)
{
	echo("ND: Client " @ %this.getPlayerName() @ " doesn't have the New Duplicator.");
	messageClient(%this, '', "\c6Hey, it looks like you don't have the \c3New Duplicator\c6 yet! Check it out: [<a:forum.blockland.us>Forum Topic</a>\c6]");
}

//Client responded, so he has new duplicator
function serverCmdNdHandshake(%this, %clientVersion)
{
	cancel(%this.ndHandshakeTimeout);

	%this.ndClient = true;
	%this.ndVersion = %clientVersion;

	switch(ND_CompareVersion($ND::Version, %clientVersion))
	{
		case 0:
			echo("ND: Client " @ %this.getPlayerName() @ " has the same version of the \c3New Duplicator\c6. (" @ %clientVersion @ ");");

		case 1:
			messageClient(%this, '', "\c6Your version of the \c3New Duplicator\c6 is outdated! Some features might not work. (Server Version: \c3" @ $ND::Version @ "\c6 | Your Version: \c0" @ %clientVersion @ "\c6)");
			echo("ND: Client " @ %this.getPlayerName() @ " has old version of New Duplicator (" @ %clientVersion @ " vs " @ $ND::Version @ ")");

		case 2:
			messageClient(%this, '', "\c6Your version of the \c3New Duplicator\c6 is newer than the server! Tell the host to update it. (Server Version: \c0" @ $ND::Version @ "\c6 | Your Version: \c3" @ %clientVersion @ "\c6)");
			echo("ND: Client " @ %this.getPlayerName() @ " has NEWER version of New Duplicator! Consider updating. (" @ %clientVersion @ " vs " @ $ND::Version @ ")");
	}
}
