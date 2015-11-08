// * ######################################################################
// *
// *    New Duplicator - Client
// *    Handshake
// *
// *    -------------------------------------------------------------------
// *    Perform a handshake with the server-sided mod on joining a servers
// *
// * ######################################################################

//Receive handshake from server
function clientCmdNdHandshake(%this, %serverVersion)
{
	$ND::ServerVersion = %serverVersion;
	$ND::ServerHasND = true;

	commandToServer('ndHandshake', $ND::Version);
}

package NewDuplicator_Client
{
	function disconnectedCleanup()
	{
		parent::disconnectedCleanup();

		$ND::ServerVersion = "0.0.0";
		$ND::ServerHasND = false;
	}
};
