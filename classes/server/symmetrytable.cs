// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_SymmetryTable
// *
// *    -------------------------------------------------------------------
// *    Detect and store symmetry planes of bricks to mirror selections
// *
// * ######################################################################


//Create symmetry table
function ND_SymmetryTable()
{
    ND_ServerGroup.add(
        %this = new ScriptObject(ND_SymmetryTable)
    );

    return %this;
}

//Delete symmetry data
function ND_SymmetryTable::deleteData(%this)
{
    deleteVariables("$ND::Symmetry*");
}

//Rebuild the symmetry table
function ND_SymmetryTable::buildTable(%this)
{
    %this.deleteData();
    deleteVariables("$TMP::*");

    %start = getRealTime();
    $ND::SymmetryCubicCount = 0;
    $ND::SymmetryMeshCount = 0;

    $TMP::AsymCountTotal = 0;
    $TMP::ZAsymCountTotal = 0;

    echo("ND: Start building brick symmetry table...");
    echo("==========================================================================");

    %count = getDatablockGroupSize();
    for(%i = 0; %i < %count; %i++)
    {
        %db = getDatablock(%i);

        if(%db.getClassName() $= "FxDtsBrickData")
            %this.processDatablock(%db);
    }

    %cubic = $ND::SymmetryCubicCount;
    %mesh = $ND::SymmetryMeshCount;

    %asym = $TMP::AsymCountTotal;
    %asymz = $TMP::ZAsymCountTotal;

    echo("==========================================================================");
    echo("ND: Finished basic tests: " @ %cubic @ " cubic, " @ %mesh @ " with mesh, " @ %asym @ " asymmetric, " @ %asymz @ " z-asymmetric");
    echo("ND: Starting X symmetric pair search...");
    echo("==========================================================================");

    for(%i = 0; %i < $TMP::AsymCountTotal; %i++)
    {
        %index = $TMP::AsymBrick[%i];

        if(!$TMP::skipAsym[%index])
            %this.processPair(%index);
    }

    echo("==========================================================================");
    echo("ND: Finished finding symmetric pairs");
    echo("ND: Starting Z symmetric pair search...");
    echo("==========================================================================");

    for(%i = 0; %i < $TMP::ZAsymCountTotal; %i++)
    {
        %index = $TMP::ZAsymBrick[%i];

        if(!$TMP::skipAsymZ[%index])
            %this.processZPair(%index);
    }
    
    deleteVariables("$TMP::*");

    echo("==========================================================================");
    echo("ND: Finished finding Z symmetric pairs");
    echo("ND: Symmetry table complete in " @ (getRealTime() - %start) / 1000 @ " seconds");
}

//Detect symmetry of a single blb file
function ND_SymmetryTable::processDatablock(%this, %datablock)
{
    //Load blb file
    %file = new FileObject();
    %file.openForRead(%datablock.brickFile);

    //Skip brick size
    %file.readLine();

    //Cubic bricks are always fully symmetric
    if(%file.readLine() $= "BRICK")
    {
        $ND::SymmetryCubicCount++;
        $ND::Symmetry[%datablock] = 1;
        $ND::SymmetryZ[%datablock] = true;

        %file.close();
        %file.delete();
        return;
    }
    
    ///////////////////////////////////////////////////
    // Load mesh
    ///////////////////////////////////////////////////
    %dbi = $ND::SymmetryMeshCount++;
    %faces = 0;
    %points = 0;

    while(!%file.isEOF())
    {
        //Find start of face
        %line = %file.readLine();

        if(getSubStr(%line, 0, 4) $= "TEX:")
        {
            %tex = getSubStr(%line, 4, strLen(%line));
            
            //Top and bottom faces have different topology, skip
            if(%tex $= "TOP" || %tex $= "BOTTOMLOOP" || %tex $= "BOTTOMEDGE")
                continue;

            //Add face
            $TMP::faceTex[%dbi, %faces] = (%tex $= "SIDE" ? 0 : (%tex $= "RAMP" ? 1 : 2));

            //Skip useless lines
            while(%file.readLine() !$= "POSITION:") {}

            //Add the 4 points
            for(%i = 0; %i < 4; %i++)
            {
                //Read next line
                %line = %file.readLine();

                //Skip useless blank lines
                while(!strLen(%line))
                    %line = %file.readLine();

                //Remove formatting from point
                %pt = vectorAdd(%line, "0 0 0");

                //Round down two digits to fix float errors
                %pt = mFloatLength(getWord(%pt, 0), 4) * 1.0
                  SPC mFloatLength(getWord(%pt, 1), 4) * 1.0
                  SPC mFloatLength(getWord(%pt, 2), 4) * 1.0;

                //Get index of this point
                if(!%ptIndex = $TMP::pointAt[%dbi, %pt])
                {
                    //New point... add to list
                    %points++;
                    %ptIndex = %points;
                    $TMP::position[%dbi, %points] = %pt;
                    $TMP::pointAt[%dbi, %pt] = %points;
                }

                //Add face to point
                if(!$TMP::pointInFace[%dbi, %faces, %ptIndex])
                    $TMP::faceAt[%dbi, %ptIndex, $TMP::facesAt[%dbi, %ptIndex]++ - 1] = %faces;

                //Add point to face
                $TMP::facePoint[%dbi, %faces, %i] = %ptIndex;
                $TMP::pointInFace[%dbi, %faces, %ptIndex] = true;
            }

            //Added face
            %faces++;
        }
    }

    $TMP::faceCount[%dbi] = %faces;
    $TMP::pointCount[%dbi] = %points;
    $TMP::datablock[%dbi] = %datablock;

    %file.close();
    %file.delete();

    //Assume brick is fully symmetric
    %failX  = false;
    %failY  = false;
    %failZ  = false;
    %failXY = false;
    %failYX = false;

    //Possible symmetries:
    // 0: asymmetric
    // 1: x & y
    // 2: x
    // 3: y
    // 4: x+y
    // 5: x-y

    //We will test in the following order:
    // X
    // Y
    // Z
    // X+Y
    // X-Y

    %failX = %this.symmetryPlaneTest(%dbi, 0);
    %failY = %this.symmetryPlaneTest(%dbi, 1);
    %failZ = %this.symmetryPlaneTest(%dbi, 2);

    if(%failX && %failY)
        %failXY = %this.symmetryPlaneTest(%dbi, 3);

    if(%failXY)
        %failYX = %this.symmetryPlaneTest(%dbi, 4);

    ///////////////////////////////////////////////////
    // Tests finished
    ///////////////////////////////////////////////////

    //X, Y symmetry
    if(!%failX && !%failY)
        %sym = 1;
    else if(!%failX)
        %sym = 2;
    else if(!%failY)
        %sym = 3;
    else if(!%failXY)
        %sym = 4;
    else if(!%failYX)
        %sym = 5;
    else
    {
        %sym = 0;

        //Add to lookup table of asymmetric bricks of this type
        //These will be used to find asymmetric pairs
        %l = $TMP::AsymCount[%faces, !%failZ]++;
        $TMP::AsymBrick[%faces, !%failZ, %l] = %dbi;

        //Add to list of asymmetric bricks
        $TMP::AsymBrick[$TMP::AsymCountTotal] = %dbi;
        $TMP::AsymCountTotal++;
    }

    $ND::Symmetry[%datablock] = %sym;


    //Z symmetry
    if(!%failZ)
        $ND::SymmetryZ[%datablock] = true;
    else
    {
        $ND::SymmetryZ[%datablock] = false;

        //Add to lookup table of Z-asymmetric bricks of this type
        //These will be used to find asymmetric pairs
        %l = $TMP::ZAsymCount[%faces, %sym]++;
        $TMP::ZAsymBrick[%faces, %sym, %l] = %dbi;

        //Add to list of Z-asymmetric bricks
        $TMP::ZAsymBrick[$TMP::ZAsymCountTotal] = %dbi;
        $TMP::ZAsymCountTotal++;
    }
}

//Find symmetric pair between two bricks
function ND_SymmetryTable::processPair(%this, %dbi)
{
    if($TMP::skipAsym[%dbi])
        return;

    %datablock = $TMP::datablock[%dbi];

    %zsym = $ND::SymmetryZ[%datablock];
    %faces = $TMP::faceCount[%dbi];
    %count = $TMP::AsymCount[%faces, %zsym];

    //Only potential match is the brick itself - fail
    if(%count == 1)
    {
        echo("No X match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
        return;
    }

    %off = -1;

    for(%i = 1; %i <= %count; %i++)
    {
        %other = $TMP::AsymBrick[%faces, %zsym, %i];

        //Don't compare with itself... won't be symmetric
        if(%other == %dbi)
            continue;

        //Test all 4 possible rotations
        //Not using loop due to lack of goto command
        if(!%this.symmetryPlaneTest2(%dbi, %other, 0, 0))
        {
            %off = 0;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 0, 1))
        {
            %off = 1;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 0, 2))
        {
            %off = 2;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 0, 3))
        {
            %off = 3;
            break;
        }
    }

    if(%off != -1)
    {
        %otherdb = $TMP::datablock[%other];
        $TMP::skipAsym[%other] = true;
        $TMP::skipAsym[%dbi] = true;
        //echo(%datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ " is symmetric to " @ %otherdb.category @ "/" @ %otherdb.subCategory @ "/" @ %otherdb.uiname @ ", offset " @ %off);
    }
    else
        echo("No X match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
}

//Find symmetric pair between two bricks
function ND_SymmetryTable::processZPair(%this, %dbi)
{
    if($TMP::skipAsymZ[%dbi])
        return;

    %datablock = $TMP::datablock[%dbi];

    %sym = $ND::Symmetry[%datablock];
    %faces = $TMP::faceCount[%dbi];
    %count = $TMP::ZAsymCount[%faces, %sym];

    //Only potential match is the brick itself - fail
    if(%count == 1)
    {
        echo("No Z match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
        return;
    }

    %off = -1;

    for(%i = 1; %i <= %count; %i++)
    {
        %other = $TMP::ZAsymBrick[%faces, %sym, %i];

        //Don't compare with itself... won't be symmetric
        if(%other == %dbi)
            continue;

        //Test all 4 possible rotations
        //Not using loop due to lack of goto command
        if(!%this.symmetryPlaneTest2(%dbi, %other, 1, 0))
        {
            %off = 0;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 1, 1))
        {
            %off = 1;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 1, 2))
        {
            %off = 2;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 1, 3))
        {
            %off = 3;
            break;
        }

        //Some upside down versions are just rotated on X or Y - try these too
        if(!%this.symmetryPlaneTest2(%dbi, %other, 2, 0))
        {
            %off = 4;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 2, 1))
        {
            %off = 5;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 2, 2))
        {
            %off = 6;
            break;
        }

        if(!%this.symmetryPlaneTest2(%dbi, %other, 2, 3))
        {
            %off = 7;
            break;
        }
    }

    if(%off != -1)
    {
        %otherdb = $TMP::datablock[%other];
        $TMP::skipAsymZ[%other] = true;
        $TMP::skipAsymZ[%dbi] = true;
        //echo(%datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ " is Z symmetric to " @ %otherdb.category @ "/" @ %otherdb.subCategory @ "/" @ %otherdb.uiname @ ", offset " @ %off);
    }
    else
    {
        echo("No Z match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
       // for(%i = 1; %i <= %count; %i++)
        //{
        //    %otherdb = $TMP::datablock[$TMP::ZAsymBrick[%faces, %sym, %i]];
        //    echo("   - " @ %otherdb.getName() @ " (" @ %otherdb.category @ "/" @ %otherdb.subCategory @ "/" @ %otherdb.uiname @ ")");
       // }
    }
}

//Test a mesh for a single symmetry plane in itself
function ND_SymmetryTable::symmetryPlaneTest(%this, %dbi, %plane)
{
    %fail = false;
    %faces = $TMP::faceCount[%dbi];

    for(%i = 0; %i < %faces; %i++)
    {
        //If this face was already used by another mirror, skip
        if(%skipFace[%i])
            continue;

        //Attempt to find the mirrored points
        for(%j = 0; %j < 4; %j++)
        {
            %pt = $TMP::facePoint[%dbi, %i, %j];

            //Do we already know the mirrored one?
            if(%mirrPt[%pt])
            {
                %mirr[%j] = %mirrPt[%pt];
                continue;
            }

            //Get position of point
            %v = $TMP::position[%dbi, %pt];

            //Get point at mirrored position based on plane
            switch$(%plane)
            {
                //Flip X
                case 0:  %mirr = $TMP::pointAt[%dbi, -firstWord(%v) SPC restWords(%v)];
                //Flip Y
                case 1:  %mirr = $TMP::pointAt[%dbi, getWord(%v, 0) SPC -getWord(%v, 1) SPC getWord(%v, 2)];
                //Flip Z
                case 2:  %mirr = $TMP::pointAt[%dbi, getWords(%v, 0, 1) SPC -getWord(%v, 2)];
                //Mirror along X+Y
                case 3:  %mirr = $TMP::pointAt[%dbi, -getWord(%v, 1) SPC -getWord(%v, 0) SPC getWord(%v, 2)];
                //Mirror along X-Y
                default: %mirr = $TMP::pointAt[%dbi, getWord(%v, 1) SPC getWord(%v, 0) SPC getWord(%v, 2)];
            }

            if(%mirr)
            {
                %mirrPt[%pt] = %mirr;
                %mirrPt[%mirr] = %pt;

                %mirr[%j] = %mirr;
            }
            else
            {
                %fail = true;
                break;
            }
        }

        if(%fail)
            break;

        //Test whether the points have a common face
        %fail = true;
        %count = $TMP::facesAt[%dbi, %mirr0];

        for(%j = 0; %j < %count; %j++)
        {
            %potentialFace = $TMP::faceAt[%dbi, %mirr0, %j];

            //Mirrored face must have the same texture id
            if($TMP::faceTex[%dbi, %i] != $TMP::faceTex[%dbi, %potentialFace])
                continue;

            //Check whether remaining points are in the face
            if(!$TMP::pointInFace[%dbi, %potentialFace, %mirr1])
                continue;

            if(!$TMP::pointInFace[%dbi, %potentialFace, %mirr2])
                continue;

            if(!$TMP::pointInFace[%dbi, %potentialFace, %mirr3])
                continue;

            //We found a matching face!
            %skipFace[%potentialFace] = true;
            %fail = false;
            break;
        }

        if(%fail)
            break;
    }

    return %fail;
}

//Test X or Z symmetry between two meshes with rotation offset
function ND_SymmetryTable::symmetryPlaneTest2(%this, %dbi, %other, %plane, %rotation)
{
    %fail = false;
    %faces = $TMP::faceCount[%dbi];

    for(%i = 0; %i < %faces; %i++)
    {
        //Attempt to find the mirrored points
        for(%j = 0; %j < 4; %j++)
        {
            %pt = $TMP::facePoint[%dbi, %i, %j];

            //Do we already know the mirrored one?
            if(%mirrPt[%pt])
            {
                %mirr[%j] = %mirrPt[%pt];
                continue;
            }

            //Get position of point
            %v = $TMP::position[%dbi, %pt];

            //0 = X, 1 = Z, 2 = X & Z
            if(%plane == 0)
            {
                //Get point at mirrored position based on rotation
                switch(%rotation)
                {
                    //Flip X
                    case 0:  %mirr = $TMP::pointAt[%other, -firstWord(%v) SPC restWords(%v)];
                    //Flip X, rotate 90
                    case 1:  %mirr = $TMP::pointAt[%other, getWord(%v, 1) SPC getWord(%v, 0) SPC getWord(%v, 2)];
                    //Flip X, rotate 180
                    case 2:  %mirr = $TMP::pointAt[%other, getWord(%v, 0) SPC -getWord(%v, 1) SPC getWord(%v, 2)];
                    //Flip X, rotate 270
                    default: %mirr = $TMP::pointAt[%other, -getWord(%v, 1) SPC -getWord(%v, 0) SPC getWord(%v, 2)];
                }
            }
            else if(%plane == 1)
            {
                //Get point at mirrored position based on rotation
                switch(%rotation)
                {
                    //Flip Z
                    case 0:  %mirr = $TMP::pointAt[%other, getWord(%v, 0) SPC getWord(%v, 1) SPC -getWord(%v, 2)];
                    //Flip Z, rotate 90
                    case 1:  %mirr = $TMP::pointAt[%other, getWord(%v, 1) SPC -getWord(%v, 0) SPC -getWord(%v, 2)];
                    //Flip Z, rotate 180
                    case 2:  %mirr = $TMP::pointAt[%other, -getWord(%v, 0) SPC -getWord(%v, 1) SPC -getWord(%v, 2)];
                    //Flip Z, rotate 270
                    default: %mirr = $TMP::pointAt[%other, -getWord(%v, 1) SPC getWord(%v, 0) SPC -getWord(%v, 2)];
                }
            }
            else
            {
                //Get point at mirrored position based on rotation
                switch(%rotation)
                {
                    //Flip X, Z
                    case 0:  %mirr = $TMP::pointAt[%other, -getWord(%v, 0) SPC getWord(%v, 1) SPC -getWord(%v, 2)];
                    //Flip X, Z, rotate 90
                    case 1:  %mirr = $TMP::pointAt[%other, getWord(%v, 1) SPC getWord(%v, 0) SPC -getWord(%v, 2)];
                    //Flip X, Z, rotate 180
                    case 2:  %mirr = $TMP::pointAt[%other, getWord(%v, 0) SPC -getWord(%v, 1) SPC -getWord(%v, 2)];
                    //Flip X, Z, rotate 270
                    default: %mirr = $TMP::pointAt[%other, -getWord(%v, 1) SPC -getWord(%v, 0) SPC -getWord(%v, 2)];
                }
            }

            if(%mirr)
            {
                %mirrPt[%pt] = %mirr;
                %mirr[%j] = %mirr;
            }
            else
            {
                %fail = true;
                break;
            }
        }

        if(%fail)
            break;

        //Test whether the points have a common face
        %fail = true;
        %count = $TMP::facesAt[%other, %mirr0];

        for(%j = 0; %j < %count; %j++)
        {
            %potentialFace = $TMP::faceAt[%other, %mirr0, %j];

            //Mirrored face must have the same texture id
            if($TMP::faceTex[%dbi, %i] != $TMP::faceTex[%other, %potentialFace])
                continue;

            //Check whether remaining points are in the face
            if(!$TMP::pointInFace[%other, %potentialFace, %mirr1])
                continue;

            if(!$TMP::pointInFace[%other, %potentialFace, %mirr2])
                continue;

            if(!$TMP::pointInFace[%other, %potentialFace, %mirr3])
                continue;

            //We found a matching face!
            %fail = false;
            break;
        }

        if(%fail)
            break;
    }

    return %fail;
}
