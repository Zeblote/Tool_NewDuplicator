// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_SelectionBox
// *
// *    -------------------------------------------------------------------
// *    Resizable and recolorable box using 14 static shapes
// *
// * ######################################################################

//Create a new selection box
function ND_SelectionBox(%shapeName)
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_SelectionBox)
	);

	%this.innerCube = new StaticShape(){datablock = ND_SelectionCubeInner;};
	%this.outerCube = new StaticShape(){datablock = ND_SelectionCubeOuter;};
	%this.shapeName = new StaticShape(){datablock = ND_SelectionCubeShapeName;};

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i] = new StaticShape(){datablock = ND_SelectionCubeBorder;};
		%this.border_y[%i] = new StaticShape(){datablock = ND_SelectionCubeBorder;};
		%this.border_z[%i] = new StaticShape(){datablock = ND_SelectionCubeBorder;};
	}

	%this.shapeName.setShapeName(%shapeName);

	%this.innerColor  = "0 0 0 0.85";
	%this.outerColor  = "1 0.84 0 0.4";
	%this.borderColor = "1 0.84 0 1";

	%this.innerColorSelected  = "1 0 0 0.85";
	%this.outerColorSelected  = "1 0 0 0.4";
	%this.borderColorSelected = "1 0 0 1";

	%this.applyColors();

	return %this;
}

//Destroy static shapes when selection box is removed
function ND_SelectionBox::onRemove(%this)
{
	%this.innerCube.delete();
	%this.outerCube.delete();
	%this.shapeName.delete();

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].delete();
		%this.border_y[%i].delete();
		%this.border_z[%i].delete();
	}
}

//Apply color changes to the selection box
function ND_SelectionBox::applyColors(%this)
{
	%this.innerCube.setNodeColor("ALL", %this.innerColor);
	%this.outerCube.setNodeColor("ALL", %this.outerColor);

	%this.shapeName.setShapeNameColor(%this.outerColor);

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].setNodeColor("ALL", %this.borderColor);
		%this.border_y[%i].setNodeColor("ALL", %this.borderColor);
		%this.border_z[%i].setNodeColor("ALL", %this.borderColor);
	}

	if(%this.selectedSide)
	{
		switch(%this.selectedSide)
		{
			case 1: %str = "+X";
			case 2: %str = "+Y";
			case 3: %str = "+Z";
			case 4: %str = "-X";
			case 5: %str = "-Y";
			case 6: %str = "-Z";
		}

		%this.innerCube.setNodeColor("in" @ %str, %this.innerColorSelected);
		%this.outerCube.setNodeColor("out" @ %str, %this.outerColorSelected);

		%bColor = %this.borderColorSelected;

		switch(%this.selectedSide)
		{
		case 1: //+X
			%this.border_y1.setNodeColor("ALL", %bColor);
			%this.border_y2.setNodeColor("ALL", %bColor);
			%this.border_z1.setNodeColor("ALL", %bColor);
			%this.border_z2.setNodeColor("ALL", %bColor);

		case 2: //+Y
			%this.border_x1.setNodeColor("ALL", %bColor);
			%this.border_x2.setNodeColor("ALL", %bColor);
			%this.border_z2.setNodeColor("ALL", %bColor);
			%this.border_z3.setNodeColor("ALL", %bColor);

		case 3: //+Z
			%this.border_x2.setNodeColor("ALL", %bColor);
			%this.border_x3.setNodeColor("ALL", %bColor);
			%this.border_y2.setNodeColor("ALL", %bColor);
			%this.border_y3.setNodeColor("ALL", %bColor);

		case 4: //-X
			%this.border_y0.setNodeColor("ALL", %bColor);
			%this.border_y3.setNodeColor("ALL", %bColor);
			%this.border_z0.setNodeColor("ALL", %bColor);
			%this.border_z3.setNodeColor("ALL", %bColor);

		case 5: //-Y
			%this.border_x0.setNodeColor("ALL", %bColor);
			%this.border_x3.setNodeColor("ALL", %bColor);
			%this.border_z0.setNodeColor("ALL", %bColor);
			%this.border_z1.setNodeColor("ALL", %bColor);

		case 6: //-Z
			%this.border_x0.setNodeColor("ALL", %bColor);
			%this.border_x1.setNodeColor("ALL", %bColor);
			%this.border_y0.setNodeColor("ALL", %bColor);
			%this.border_y1.setNodeColor("ALL", %bColor);
		}
	}
}

//Return current size of selection box
function ND_SelectionBox::getSize(%this)
{
	return %this.point1 SPC %this.point2;
}

//Resize the selection box
function ND_SelectionBox::resize(%this, %point1, %point2)
{
	if(getWordCount(%point1) == 6)
	{
		%point2 = getWords(%point1, 3, 5);
		%point1 = getWords(%point1, 0, 2);
	}

	%this.point1 = %point1;
	%this.point2 = %point2;

	%x1 = getWord(%point1, 0);
	%y1 = getWord(%point1, 1);
	%z1 = getWord(%point1, 2);

	%x2 = getWord(%point2, 0);
	%y2 = getWord(%point2, 1);
	%z2 = getWord(%point2, 2);

	%len_x = %x2 - %x1;
	%len_y = %y2 - %y1;
	%len_z = %z2 - %z1;

	%center_x = (%x1 + %x2) / 2;
	%center_y = (%y1 + %y2) / 2;
	%center_z = (%z1 + %z2) / 2;

	%rot_x = "0 1 0 1.57079";
	%rot_y = "1 0 0 1.57079";
	%rot_z = "0 0 1 0";

	%this.innerCube.setTransform(%center_x SPC %center_y SPC %center_z);
	%this.outerCube.setTransform(%center_x SPC %center_y SPC %center_z);
	%this.shapeName.setTransform(%center_X SPC %center_y SPC %z2);

	%this.border_x0.setTransform(%center_x SPC %y1 SPC %z1 SPC %rot_x);
	%this.border_x1.setTransform(%center_x SPC %y2 SPC %z1 SPC %rot_x);
	%this.border_x2.setTransform(%center_x SPC %y2 SPC %z2 SPC %rot_x);
	%this.border_x3.setTransform(%center_x SPC %y1 SPC %z2 SPC %rot_x);

	%this.border_y0.setTransform(%x1 SPC %center_y SPC %z1 SPC %rot_y);
	%this.border_y1.setTransform(%x2 SPC %center_y SPC %z1 SPC %rot_y);
	%this.border_y2.setTransform(%x2 SPC %center_y SPC %z2 SPC %rot_y);
	%this.border_y3.setTransform(%x1 SPC %center_y SPC %z2 SPC %rot_y);

	%this.border_z0.setTransform(%x1 SPC %y1 SPC %center_z SPC %rot_z);
	%this.border_z1.setTransform(%x2 SPC %y1 SPC %center_z SPC %rot_z);
	%this.border_z2.setTransform(%x2 SPC %y2 SPC %center_z SPC %rot_z);
	%this.border_z3.setTransform(%x1 SPC %y2 SPC %center_z SPC %rot_z);

	%this.innerCube.setScale(%len_x - 0.02 SPC %len_y - 0.02 SPC %len_z - 0.02);
	%this.outerCube.setScale(%len_x + 0.02 SPC %len_y + 0.02 SPC %len_z + 0.02);

	%maxLen = getMax(getMax(%len_x, %len_y), %len_z);

	if(%maxLen > 1024)
		%width = 7;
	else if(%maxLen > 512)
		%width = 6;
	else if(%maxLen > 256)
		%width = 5;
	else if(%maxLen > 128)
		%width = 4;
	else if(%maxLen > 64)
		%width = 3;
	else if(%maxLen > 32)
		%width = 2;
	else if(%maxLen > 4)
		%width = 1;
	else
		%width = 0.5;

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].setScale(%width SPC %width SPC %len_x + %width * 0.05);
		%this.border_y[%i].setScale(%width SPC %width SPC %len_y + %width * 0.05);
		%this.border_z[%i].setScale(%width SPC %width SPC %len_z + %width * 0.05);
	}
}

//Select a side of the box
function ND_SelectionBox::selectSide(%this, %side)
{
	if(%this.selectedSide != %side)
	{
		%this.selectedSide = %side;
		%this.applyColors();

		serverPlay3d(BrickRotateSound, %this.getSelectedSideCenter());
	}
}

//Deselect the side
function ND_SelectionBox::deselectSide(%this)
{
	if(%this.selectedSide != 0)
	{
		serverPlay3d(BrickRotateSound, %this.getSelectedSideCenter());

		%this.selectedSide = 0;
		%this.applyColors();
	}
}

//Move the selected side of the box in or out
function ND_SelectionBox::stepSide(%this, %distance, %limit)
{
	if(!%this.selectedSide)
		return;

	%distance = mFloor(%distance);
	%oldP1 = %this.point1;
	%oldP2 = %this.point2;

	switch(%this.selectedSide)
	{
		case 1: %newPoint = vectorAdd(%this.point2, vectorScale("0.5 0 0", %distance));
		case 2: %newPoint = vectorAdd(%this.point2, vectorScale("0 0.5 0", %distance));
		case 3: %newPoint = vectorAdd(%this.point2, vectorScale("0 0 0.2", %distance));
		case 4: %newPoint = vectorSub(%this.point1, vectorScale("0.5 0 0", %distance));
		case 5: %newPoint = vectorSub(%this.point1, vectorScale("0 0.5 0", %distance));
		case 6: %newPoint = vectorSub(%this.point1, vectorScale("0 0 0.2", %distance));
	}

	//Validate box size
	if(%this.selectedSide > 3)
	{
		%offset = vectorSub(%this.point2, %newPoint);

		%x = mClampF(getWord(%offset, 0), 0.5, %limit);
		%y = mClampF(getWord(%offset, 1), 0.5, %limit);
		%z = mClampF(getWord(%offset, 2), 0.2, %limit);

		%newOffset = %x SPC %y SPC %z;
		%this.point1 = vectorSub(%this.point2, %newOffset);

		if(vectorLen(%newOffset) < vectorLen(%offset))
			%limitReached = true;
	}
	else
	{
		%offset = vectorSub(%newPoint, %this.point1);

		%x = mClampF(getWord(%offset, 0), 0.5, %limit);
		%y = mClampF(getWord(%offset, 1), 0.5, %limit);
		%z = mClampF(getWord(%offset, 2), 0.2, %limit);

		%newOffset = %x SPC %y SPC %z;
		%this.point2 = vectorAdd(%this.point1, %newOffset);

		if(vectorLen(%newOffset) < vectorLen(%offset))
			%limitReached = true;
	}

	if(%this.point1 !$= %oldP1 || %this.point2 !$= %oldP2)
	{
		%this.resize(%this.point1, %this.point2);
		serverPlay3d(BrickMoveSound, %this.getSelectedSideCenter());
	}
	else
		serverPlay3d(errorSound, %this.getSelectedSideCenter());

	return %limitReached;
}

//Get center of currently selected side
function ND_SelectionBox::getSelectedSideCenter(%this)
{
	%halfDiag = vectorSub(%this.point2, %this.point1);

	%halfX = getWord(%halfDiag, 0);
	%halfY = getWord(%halfDiag, 1);
	%halfZ = getWord(%halfDiag, 2);

	switch(%this.selectedSide)
	{
		case 1: return vectorSub(%this.point2, 0      SPC %halfY SPC %halfZ);
		case 2: return vectorSub(%this.point2, %halfX SPC 0      SPC %halfZ);
		case 3: return vectorSub(%this.point2, %halfX SPC %halfY SPC 0     );
		case 4: return vectorAdd(%this.point1, 0      SPC %halfY SPC %halfZ);
		case 5: return vectorAdd(%this.point1, %halfX SPC 0      SPC %halfZ);
		case 6: return vectorAdd(%this.point1, %halfX SPC %halfY SPC 0     );
	}
}
