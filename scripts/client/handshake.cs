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
function clientCmdNdHandshake(%this, %serverVersion)
{
	$ND::ServerVersion = %serverVersion;
	$ND::ServerHasND = true;

	commandToServer('ndHandshake', $ND::Version);
}

package NewDuplicator_Client
{
	//Reset server version on leaving server
	function disconnectedCleanup()
	{
		$ND::ServerVersion = "0.0.0";
		$ND::ServerHasND = false;
		
		parent::disconnectedCleanup();
	}
};
