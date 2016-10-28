// Converts integers to characters and back. Mainly used by save transfer.
// -------------------------------------------------------------------

//Binary compression (file version, 241 allowed characters)
///////////////////////////////////////////////////////////////////////////

//Creates byte lookup table
function ndCreateByte241Table()
{
	$ND::Byte241Lookup = "";

	//This will map uints 0-241 to chars 15-255, starting after \r
	for(%i = 15; %i < 256; %i++)
	{
		%char = collapseEscape("\\x" @
			getSubStr("0123456789abcdef", (%i & 0xf0) >> 4, 1) @
			getSubStr("0123456789abcdef", %i & 0x0f, 1));

		$ND::Byte241ToChar[%i - 15] = %char;
		$ND::Byte241Lookup = $ND::Byte241Lookup @ %char;
	}

	$ND::Byte241TableCreated = true;
}

//Packs uint in single byte
function ndPack241_1(%num)
{
	return $ND::Byte241ToChar[%num];
}

//Packs uint in two bytes
function ndPack241_2(%num)
{
	return $ND::Byte241ToChar[(%num / 241) | 0] @ $ND::Byte241ToChar[%num % 241];
}

//Packs uint in three bytes
function ndPack241_3(%num)
{
	return
		$ND::Byte241ToChar[(((%num / 241) | 0) / 241) | 0] @
		$ND::Byte241ToChar[((%num / 241) | 0) % 241] @
		$ND::Byte241ToChar[%num % 241];
}

//Packs uint in four bytes
function ndPack241_4(%num)
{
	return
		$ND::Byte241ToChar[(((((%num / 241) | 0) / 241) | 0) / 241) | 0] @
		$ND::Byte241ToChar[((((%num / 241) | 0) / 241) | 0) % 241] @
		$ND::Byte241ToChar[((%num / 241) | 0) % 241] @
		$ND::Byte241ToChar[%num % 241];
}

//Unpacks uint from single byte
function ndUnpack241_1(%subStr)
{
	return strStr($ND::Byte241Lookup, %subStr);
}

//Unpacks uint from two bytes
function ndUnpack241_2(%subStr)
{
	return
		strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 241 +
		strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1));
}

//Unpacks uint from three bytes
function ndUnpack241_3(%subStr)
{
	return
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 58081) | 0) +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1)) *   241       +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 2, 1));
}

//Unpacks uint from four bytes
function ndUnpack241_4(%subStr)
{
	return
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 13997521) | 0) +
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1)) *    58081) | 0) +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 2, 1)) *      241       +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 3, 1));
}



//Binary compression (command version, 255 allowed characters)
///////////////////////////////////////////////////////////////////////////

//Creates byte lookup table
function ndCreateByte255Table()
{
	$ND::Byte255Lookup = "";

	//This will map uints 0-254 to chars 1-255, starting after \x00
    for(%i = 1; %i < 256; %i++)
    {
        %char = collapseEscape("\\x" @
          getSubStr("0123456789abcdef", (%i & 0xf0) >> 4, 1) @
          getSubStr("0123456789abcdef", %i & 0x0f, 1));

        $ND::Byte255ToChar[%i - 1] = %char;
        $ND::Byte255Lookup = $ND::Byte255Lookup @ %char;
    }

	$ND::Byte255TableCreated = true;
}

//Packs uint in single byte
function ndPack255_1(%num)
{
	return $ND::Byte255ToChar[%num];
}

//Packs uint in two bytes
function ndPack255_2(%num)
{
	return $ND::Byte255ToChar[(%num / 255) | 0] @ $ND::Byte255ToChar[%num % 255];
}

//Packs uint in three bytes
function ndPack255_3(%num)
{
	return
		$ND::Byte255ToChar[(((%num / 255) | 0) / 255) | 0] @
		$ND::Byte255ToChar[((%num / 255) | 0) % 255] @
		$ND::Byte255ToChar[%num % 255];
}

//Packs uint in four bytes
function ndPack255_4(%num)
{
	return
		$ND::Byte255ToChar[(((((%num / 255) | 0) / 255) | 0) / 255) | 0] @
		$ND::Byte255ToChar[((((%num / 255) | 0) / 255) | 0) % 255] @
		$ND::Byte255ToChar[((%num / 255) | 0) % 255] @
		$ND::Byte255ToChar[%num % 255];
}

//Unpacks uint from single byte
function ndUnpack255_1(%subStr)
{
	return strStr($ND::Byte255Lookup, %subStr);
}

//Unpacks uint from two bytes
function ndUnpack255_2(%subStr)
{
	return
		strStr($ND::Byte255Lookup, getSubStr(%subStr, 0, 1)) * 255 +
		strStr($ND::Byte255Lookup, getSubStr(%subStr, 1, 1));
}

//Unpacks uint from three bytes
function ndUnpack255_3(%subStr)
{
	return
		((strStr($ND::Byte255Lookup, getSubStr(%subStr, 0, 1)) * 65025) | 0) +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 1, 1)) *   255       +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 2, 1)) | 0;
}

//Unpacks uint from four bytes
function ndUnpack255_4(%subStr)
{
	return
		((strStr($ND::Byte255Lookup, getSubStr(%subStr, 0, 1)) * 16581375) | 0) +
		((strStr($ND::Byte255Lookup, getSubStr(%subStr, 1, 1)) *    65025) | 0) +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 2, 1)) *      255       +
		  strStr($ND::Byte255Lookup, getSubStr(%subStr, 3, 1)) | 0;
}

//Some tests for the packing functions
function ndTestPack255()
{
	echo("Testing 1 byte");
	echo(ndUnpack255_1(ndPack255_1(0)) == 0);
	echo(ndUnpack255_1(ndPack255_1(123)) == 123);
	echo(ndUnpack255_1(ndPack255_1(231)) == 231);
	echo(ndUnpack255_1(ndPack255_1(254)) == 254);

	echo("Testing 2 byte");
	echo(ndUnpack255_2(ndPack255_2(0)) == 0);
	echo(ndUnpack255_2(ndPack255_2(123)) == 123);
	echo(ndUnpack255_2(ndPack255_2(231)) == 231);
	echo(ndUnpack255_2(ndPack255_2(254)) == 254);
	echo(ndUnpack255_2(ndPack255_2(12345)) == 12345);
	echo(ndUnpack255_2(ndPack255_2(32145)) == 32145);
	echo(ndUnpack255_2(ndPack255_2(65024)) == 65024);

	echo("Testing 3 byte");
	echo(ndUnpack255_3(ndPack255_3(0)) == 0);
	echo(ndUnpack255_3(ndPack255_3(123)) == 123);
	echo(ndUnpack255_3(ndPack255_3(231)) == 231);
	echo(ndUnpack255_3(ndPack255_3(254)) == 254);
	echo(ndUnpack255_3(ndPack255_3(12345)) == 12345);
	echo(ndUnpack255_3(ndPack255_3(32145)) == 32145);
	echo(ndUnpack255_3(ndPack255_3(65024)) == 65024);
	echo(ndUnpack255_3(ndPack255_3(11234567)) == 11234567);
	echo(ndUnpack255_3(ndPack255_3(14132451)) == 14132451);
	echo(ndUnpack255_3(ndPack255_3(16581374)) == 16581374);

	echo("Testing 4 byte");
	echo(ndUnpack255_4(ndPack255_4(0)) == 0);
	echo(ndUnpack255_4(ndPack255_4(123)) == 123);
	echo(ndUnpack255_4(ndPack255_4(231)) == 231);
	echo(ndUnpack255_4(ndPack255_4(254)) == 254);
	echo(ndUnpack255_4(ndPack255_4(12345)) == 12345);
	echo(ndUnpack255_4(ndPack255_4(32145)) == 32145);
	echo(ndUnpack255_4(ndPack255_4(65024)) == 65024);
	echo(ndUnpack255_4(ndPack255_4(11234567)) == 11234567);
	echo(ndUnpack255_4(ndPack255_4(14132451)) == 14132451);
	echo(ndUnpack255_4(ndPack255_4(16581374)) == 16581374);
	echo(ndUnpack255_4(ndPack255_4(1234567890)) == 1234567890);

	//Appearantly tork uses uint and normal int randomly in
	//seperate places so we can't use the full uint range
	echo(ndUnpack255_4(ndPack255_4(2147483647)) == 2147483647);
	echo(ndUnpack255_4(ndPack255_4(2147483648)) != 2147483648);
}
