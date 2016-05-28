// * ######################################################################
// *
// *    New Duplicator - Client
// *    Handshake
// *
// *    -------------------------------------------------------------------
// *    Responds to handshake request sent from server side
// *
// * ######################################################################

//Assume server doesn't have the new duplicator
$ND::ServerVersion = "0.0.0";
$ND::ServerHasND = false;

//Receive handshake from server
function clientCmdNdHandshake(%serverVersion)
{
	$ND::ServerVersion = %serverVersion;
	$ND::ServerHasND = true;

	commandToServer('ndHandshake', $ND::Version);
}

package NewDuplicator_Client
{
	//Reset server version on leaving server
	function disconnectedCleanup(%bool)
	{
		$ND::ServerVersion = "0.0.0";
		$ND::ServerHasND = false;

		//Disable the keybinds
		clientCmdNdEnableKeybinds(false);

		parent::disconnectedCleanup(%bool);
	}
};
