// * ######################################################################
// *
// *    New Duplicator - Server
// *    Handshake
// *
// *    -------------------------------------------------------------------
// *    Sends handshake request to new clients, advertises new duplicator
// *
// * ######################################################################

package NewDuplicator_Server
{
	//Send handshake request to new client
	function GameConnection::autoAdminCheck(%this)
	{
		%this.ndClient = false;
		%this.ndVersion = "0.0.0";

		commandToClient(%this, 'ndHandshake', $ND::Version);

		//Client has 5 seconds to respond
		//%this.ndHandshakeTimeout = %this.schedule(5000, "ndHandshakeTimeout");

		return parent::autoAdminCheck(%this);
	}
};

//Handshake request timed out, client doesn't have new duplicator
//function GameConnection::ndHandshakeTimeout(%this)
//{
//
//}

//Client responded, so he has new duplicator
function serverCmdNdHandshake(%this, %version)
{
	cancel(%this.ndHandshakeTimeout);

	%this.ndClient = true;
	%this.ndVersion = %version;

	//Inform client whether he has an outdated version
	switch(ndCompareVersion($ND::Version, %version))
	{
		case 1:
			%m =      "\c6Your version of the \c3New Duplicator\c6 is outdated! Some features might not work. ";
			%m = %m @ "(Server Version: \c3" @ $ND::Version @ "\c6 | Your Version: \c0" @ %version @ "\c6)";
			messageClient(%this, '', %m);

		case 2:
			//Hide this message on long-running dedicated servers
			if($Sim::Time < 86400)
			{
				%m =      "\c6Your version of the \c3New Duplicator\c6 is newer than the server's! Ask the host to update it! ";
				%m = %m @ "(Server Version: \c0" @ $ND::Version @ "\c6 | Your Version: \c3" @ %version @ "\c6)";
				messageClient(%this, '', %m);
			}
	}
}

//Compares two version numbers (major.minor.patch)
function ndCompareVersion(%ver1, %ver2)
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

//Send handshakes to all clients
function ndResendHandshakes()
{
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
		commandToClient(ClientGroup.getObject(%i), 'ndHandshake', $ND::Version);
}
