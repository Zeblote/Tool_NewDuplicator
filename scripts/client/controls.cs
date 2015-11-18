// * ######################################################################
// *
// *    New Duplicator - Client
// *    Controls
// *
// *    -------------------------------------------------------------------
// *    Repeats rotating bricks if key is held down for some time
// *
// * ######################################################################

package NewDuplicator_Client
{
	//Start/Stop rotating brick right
	function RotateBrickCW(%pressed)
	{
		if(%pressed)
			$RepeatRotateCW = schedule($BrickFirstRepeatTime, 0, "RepeatBrickCW");
		else
			cancel($RepeatRotateCW);

		parent::RotateBrickCW(%pressed);
	}

	//Start/Stop rotating brick left
	function RotateBrickCCW(%pressed)
	{
		if(%pressed)
			$RepeatRotateCCW = schedule($BrickFirstRepeatTime, 0, "RepeatBrickCCW");
		else
			cancel($RepeatRotateCCW);

		parent::RotateBrickCCW(%pressed);
	}
};

//Repeat rotating brick right
function RepeatBrickCW()
{
	commandToServer('rotateBrick', 1);

	if($RecordingBuildMacro && isObject($BuildMacroSO))
		$BuildMacroSO.pushEvent("Server", 'rotateBrick', 1);

	cancel($RepeatRotateCW);
	$RepeatRotateCW = schedule($BrickRepeatTime, 0, "RepeatBrickCW");
}

//Repeat rotating brick left
function RepeatBrickCCW()
{
	commandToServer('rotateBrick', -1);

	if($RecordingBuildMacro && isObject($BuildMacroSO))
		$BuildMacroSO.pushEvent("Server", 'rotateBrick', -1);

	cancel($RepeatRotateCCW);
	$RepeatRotateCCW = schedule($BrickRepeatTime, 0, "RepeatBrickCCW");
}
