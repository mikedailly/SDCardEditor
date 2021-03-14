using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDCardEditor
{

    public static class Basic
    {
        public static Dictionary<int, string> TokenLookup;
        /*{
    };

        const usesLineNumbers = [
          'GO SUB',
          'GO TO',
          'LIST',
          'LINE',
          'LLIST',
          'RESTORE',
          'RUN',
          'CODE',
        ];

        const bitWiseOperators = ['&', '|', '^', '!', '>>', '<<'];

        const operators = [
          'AND', // logic
          'OR',
          'NOT',
          'MOD',
          '-', // math
          '*',
          '/',
          '<', // compare
          '>',
          '<=',
          '>=',
          '<>',

          ...bitWiseOperators,
          'INT', // misc?
    ];

        const intFunctions = [
          'IN',
          'REG',
          'PEEK',
          'DPEEK',
          'USR',
          'BIN',
          'RND',
          'BANK',
          'SPRITE',
          'INT',
          'ABS',
          'SGN',

          ...operators,
        ].reduce((acc, curr) => {
            acc[curr] = true;
            return acc;
        }, {});

    intFunctions.SPRITE = ['CONTINUE', 'AT', 'OVER'];
    intFunctions.BANK = ['USR', 'PEEK', 'DPEEK'];
    intFunctions.ABS = ['*'];

    const functions = [
      'ABS',
      'ACS',
      'ASN',
      'ATN',
      'ATTR',
      'CHR$',
      'CODE',
      'COS',
      'EXP',
      'FN',
      'IN',
      'INKEY$',
      'INT',
      'LEN',
      'PEEK',
      'PEEK$',
      'PI',
      'POINTER',
      'REG',
      'RND',
      'SCREEN$',
      'SGN',
      'SIN',
      'SQR',
      'STR$',
      'TAN',
      'USR',
      'VAL',
      'VAL$',
    ].reduce((acc, curr) => {
        acc[curr] = true;
        return acc;
    }, {});

    const printModifiers = [
      'INK',
      'PAPER',
      'FLASH',
      'INVERSE',
      'OVER',
      'BRIGHT',
      'POINT',
      'AT',
      'LINE',
      'TO',
      'BIN',
      'TAB',
    ].reduce((acc, curr) => {
    */

        public static void Init()
        {
            TokenLookup = new Dictionary<int, string>();
            TokenLookup.Add(0x3a,":");
            TokenLookup.Add(0x3b,";");
            TokenLookup.Add(0x3c,"<");
            TokenLookup.Add(0x3e,">");
            //TokenLookup.Add(0x2a: "*");
            TokenLookup.Add(0x87,"PEEK$");
            TokenLookup.Add(0x88,"REG");
            TokenLookup.Add(0x89,"DPOKE");
            TokenLookup.Add(0x8a,"DPEEK");
            TokenLookup.Add(0x8b,"MOD");
            TokenLookup.Add(0x8c,"<<");
            TokenLookup.Add(0x8d,">>");
            TokenLookup.Add(0x8e,"UNTIL");
            TokenLookup.Add(0x8f,"ERROR");
            TokenLookup.Add(0x90,"ON");
            TokenLookup.Add(0x91,"DEFPROC");
            TokenLookup.Add(0x92,"ENDPROC");
            TokenLookup.Add(0x93,"PROC");
            TokenLookup.Add(0x94,"LOCAL");
            TokenLookup.Add(0x95,"DRIVER");
            TokenLookup.Add(0x96,"WHILE");
            TokenLookup.Add(0x97,"REPEAT");
            TokenLookup.Add(0x98,"ELSE");
            TokenLookup.Add(0x99,"REMOUNT");
            TokenLookup.Add(0x9a,"BANK");
            TokenLookup.Add(0x9b,"TILE");
            TokenLookup.Add(0x9c,"LAYER");
            TokenLookup.Add(0x9d,"PALETTE");
            TokenLookup.Add(0x9e,"SPRITE");
            TokenLookup.Add(0x9f,"PWD");
            TokenLookup.Add(0xa0,"CD");
            TokenLookup.Add(0xa1,"MKDIR");
            TokenLookup.Add(0xa2,"RMDIR");
            TokenLookup.Add(0xa3,"SPECTRUM");
            TokenLookup.Add(0xa4,"PLAY");
            TokenLookup.Add(0xa5,"RND");
            TokenLookup.Add(0xa6,"INKEY$");
            TokenLookup.Add(0xa7,"PI");
            TokenLookup.Add(0xa8,"FN");
            TokenLookup.Add(0xa9,"POINT");
            TokenLookup.Add(0xaa,"SCREEN$");
            TokenLookup.Add(0xab,"ATTR");
            TokenLookup.Add(0xac,"AT");
            TokenLookup.Add(0xad,"TAB");
            TokenLookup.Add(0xae,"VAL$");
            TokenLookup.Add(0xaf,"CODE");
            TokenLookup.Add(0xb0,"VAL");
            TokenLookup.Add(0xb1,"LEN");
            TokenLookup.Add(0xb2,"SIN");
            TokenLookup.Add(0xb3,"COS");
            TokenLookup.Add(0xb4,"TAN");
            TokenLookup.Add(0xb5,"ASN");
            TokenLookup.Add(0xb6,"ACS");
            TokenLookup.Add(0xb7,"ATN");
            TokenLookup.Add(0xb8,"LN");
            TokenLookup.Add(0xb9,"EXP");
            TokenLookup.Add(0xba,"INT");
            TokenLookup.Add(0xbb,"SQR");
            TokenLookup.Add(0xbc,"SGN");
            TokenLookup.Add(0xbd,"ABS");
            TokenLookup.Add(0xbe,"PEEK");
            TokenLookup.Add(0xbf,"IN");
            TokenLookup.Add(0xc0,"USR");
            TokenLookup.Add(0xc1,"STR$");
            TokenLookup.Add(0xc2,"CHR$");
            TokenLookup.Add(0xc3,"NOT");
            TokenLookup.Add(0xc4,"BIN");
            TokenLookup.Add(0xc5,"OR");
            TokenLookup.Add(0xc6,"AND");
            TokenLookup.Add(0xc7,"<=");
            TokenLookup.Add(0xc8,">=");
            TokenLookup.Add(0xc9,"<>");
            TokenLookup.Add(0xca,"LINE");
            TokenLookup.Add(0xcb,"THEN");
            TokenLookup.Add(0xcc,"TO");
            TokenLookup.Add(0xcd,"STEP");
            TokenLookup.Add(0xce,"DEF FN");
            TokenLookup.Add(0xcf,"CAT");
            TokenLookup.Add(0xd0,"FORMAT");
            TokenLookup.Add(0xd1,"MOVE");
            TokenLookup.Add(0xd2,"ERASE");
            TokenLookup.Add(0xd3,"OPEN #");
            TokenLookup.Add(0xd4,"CLOSE #");
            TokenLookup.Add(0xd5,"MERGE");
            TokenLookup.Add(0xd6,"VERIFY");
            TokenLookup.Add(0xd7,"BEEP");
            TokenLookup.Add(0xd8,"CIRCLE");
            TokenLookup.Add(0xd9,"INK");
            TokenLookup.Add(0xda,"PAPER");
            TokenLookup.Add(0xdb,"FLASH");
            TokenLookup.Add(0xdc,"BRIGHT");
            TokenLookup.Add(0xdd,"INVERSE");
            TokenLookup.Add(0xde,"OVER");
            TokenLookup.Add(0xdf,"OUT");
            TokenLookup.Add(0xe0,"LPRINT");
            TokenLookup.Add(0xe1,"LLIST");
            TokenLookup.Add(0xe2,"STOP");
            TokenLookup.Add(0xe3,"READ");
            TokenLookup.Add(0xe4,"DATA");
            TokenLookup.Add(0xe5,"RESTORE");
            TokenLookup.Add(0xe6,"NEW");
            TokenLookup.Add(0xe7,"BORDER");
            TokenLookup.Add(0xe8,"CONTINUE");
            TokenLookup.Add(0xe9,"DIM");
            TokenLookup.Add(0xea,"REM");
            TokenLookup.Add(0xeb,"FOR");
            TokenLookup.Add(0xec,"GO TO");
            TokenLookup.Add(0xed,"GO SUB");
            TokenLookup.Add(0xee,"INPUT");
            TokenLookup.Add(0xef,"LOAD");
            TokenLookup.Add(0xf0,"LIST");
            TokenLookup.Add(0xf1,"LET");
            TokenLookup.Add(0xf2,"PAUSE");
            TokenLookup.Add(0xf3,"NEXT");
            TokenLookup.Add(0xf4,"POKE");
            TokenLookup.Add(0xf5,"PRINT");
            TokenLookup.Add(0xf6,"PLOT");
            TokenLookup.Add(0xf7,"RUN");
            TokenLookup.Add(0xf8,"SAVE");
            TokenLookup.Add(0xf9,"RANDOMIZE");
            TokenLookup.Add(0xfa,"IF");
            TokenLookup.Add(0xfb,"CLS");
            TokenLookup.Add(0xfc,"DRAW");
            TokenLookup.Add(0xfd,"CLEAR");
            TokenLookup.Add(0xfe,"RETURN");
            TokenLookup.Add(0xff,"COPY");
        }

        public static string LookUp(int _value)
        {
            string val;
            if(TokenLookup.TryGetValue(_value, out val))
            {
                return val;
            }
            return "UNKOWN ";
        }

        public static void Basic2Text(byte[] _file)
        {
            FileBuffer fb = FileBuffer.Load(_file);

            StringBuilder sb = new StringBuilder(1024 * 1024);
            fb.CurrentIndex = 128;
            for (int i=128;i<_file.Length;i++)
            {
                // Line number
                int line = fb.Read16();
                int unknown = fb.Read16();

                string keyword = LookUp(fb.Read8());



            }
        }

    }
}
