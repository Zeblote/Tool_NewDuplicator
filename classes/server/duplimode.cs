// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_DupliMode
// *
// *    -------------------------------------------------------------------
// *    Abstract class for handling a single dupli mode
// *
// * ######################################################################

//Possible dupli modes, can be used as bitmask
$NDDM::Disabled               =      0;
$NDDM::StackSelect            = 1 << 0;
$NDDM::CubeSelect             = 1 << 1;
$NDDM::StackSelectProgress    = 1 << 2;
$NDDM::CubeSelectProgress     = 1 << 3;
$NDDM::PlaceCopy              = 1 << 4;

//Disabled dupli mode (does nothing)
if(isObject(NDDM_Disabled))
	NDDM_Disabled.delete();

ND_ServerGroup.add(
	new ScriptObject(NDDM_Disabled)
	{
		class = "ND_DupliMode";
		num = $NDDM::Disabled;

		allowedModes = $NDDM::StackSelect
			| $NDDM::CubeSelect;

		allowSwinging = false;
	}
);

function NDDM_Disabled::onStartMode(%this, %client, %lastMode)
{
	commandToClient(%client, 'clearBottomPrint');
}



//Changing modes
///////////////////////////////////////////////////////////////////////////

//Switch to this mode
function ND_DupliMode::onStartMode(%this, %client, %lastMode)
{

}

//Switch away from this mode
function ND_DupliMode::onChangeMode(%this, %client, %nextMode)
{

}

//Duplicator image callbacks
///////////////////////////////////////////////////////////////////////////

//Selecting an object with the duplicator
function ND_DupliMode::onSelectObject(%this, %client, %obj, %pos, %normal)
{

}

//Generic inputs
///////////////////////////////////////////////////////////////////////////

//Light key
function ND_DupliMode::onLight(%this, %client)
{

}

//Next Seat
function ND_DupliMode::onNextSeat(%this, %client)
{

}

//Prev Seat
function ND_DupliMode::onPrevSeat(%this, %client)
{

}

//Shift Brick
function ND_DupliMode::onShiftBrick(%this, %client, %x, %y, %z)
{

}

//Super Shift Brick
function ND_DupliMode::onSuperShiftBrick(%this, %client, %x, %y, %z)
{

}

//Rotate Brick
function ND_DupliMode::onRotateBrick(%this, %client, %direction)
{

}

//Plant Brick
function ND_DupliMode::onPlantBrick(%this, %client)
{

}

//Cancel Brick
function ND_DupliMode::onCancelBrick(%this, %client)
{

}

//Interface
///////////////////////////////////////////////////////////////////////////

//Build a bottomprint
function ND_DupliMode::getBottomPrint(%this, %client)
{

}
