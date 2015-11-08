// * ######################################################################
// *
// *    New Duplicator - Client
// *    Repeat rotating bricks
// *
// *    -------------------------------------------------------------------
// *    Automatically repeat rotating bricks, just like moving bricks
// *
// * ######################################################################

package NewDuplicator_Client
{
	//Catch rotating bricks right
	function RotateBrickCW(%val)
	{
		if(%val)
			$RepeatRotateCW = schedule($BrickFirstRepeatTime, 0, "RepeatBrickCW");
		else
			cancel($RepeatRotateCW);

		parent::RotateBrickCW(%val);
	}

	//Catch rotating bricks left
	function RotateBrickCCW(%val)
	{
		if(%val)
			$RepeatRotateCCW = schedule($BrickFirstRepeatTime, 0, "RepeatBrickCCW");
		else
			cancel($RepeatRotateCCW);

		parent::RotateBrickCCW(%val);
	}
};

//Repeat rotating right
function RepeatBrickCW()
{
	commandToServer('rotateBrick', 1);

	if($RecordingBuildMacro && isObject($BuildMacroSO))
		$BuildMacroSO.pushEvent("Server", 'rotateBrick', 1);

	cancel($RepeatRotateCW);
	$RepeatRotateCW = schedule($BrickRepeatTime, 0, "RepeatBrickCW");
}

//Repeat rotating left
function RepeatBrickCCW()
{
	commandToServer('rotateBrick', -1);

	if($RecordingBuildMacro && isObject($BuildMacroSO))
		$BuildMacroSO.pushEvent("Server", 'rotateBrick', -1);

	cancel($RepeatRotateCCW);
	$RepeatRotateCCW = schedule($BrickRepeatTime, 0, "RepeatBrickCCW");
}
