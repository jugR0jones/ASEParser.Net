using System;
using System.Runtime.Serialization;

namespace ASEParser
{
    public class ASEImporter
    {
        private enum TokenType
        {
            Unknown = 0,
            WhiteSpace = 1,
            String = 2,
            OpenObject = 3,
            CloseObject = 4,
            ChunkName = 5,
            //TODO: This needs to be determined in a better manner
            ChunkData = 6,
        }

        private struct Token
        {
            public TokenType tokenType;
            public string Value;
        }

        /// <summary>
        /// Colour values should be in range 0..1
        /// </summary>
        private class RGBColour
        {
            public float red;
            public float green;
            public float blue;
        }
        
        private static int filePtr;
        private static int fileLength;
        private static string fileData;
        
        private static bool IsWhiteSpace(char ch)
        {
            return ch == ' ' ||
                   ch == '\t' ||
                   ch == '\n' ||
                   ch == '\r';
        }
        
        private static bool Read(out Token token)
        {
            int tmpFilePtr = filePtr;
            bool isString = false;

            token = new Token();
            
            // trim the white space before we get to the token
            while (tmpFilePtr < fileLength)
            {
                //TODO: THIS TRIM FUNCTION SHOULD BE RETURNED AS A TOKEN
                bool isWhiteSpace = IsWhiteSpace(fileData[tmpFilePtr]);
                if (!isWhiteSpace)
                {
                    break;
                }
                
                tmpFilePtr++;
            }
            
            token.Value = "";
            while (tmpFilePtr < fileLength)
            {
                if (fileData[tmpFilePtr] == '"')
                {
                    tmpFilePtr++;
                 
                    if (isString == false)
                    {
                        isString = true;
                        token.tokenType = TokenType.String;
                        
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (fileData[tmpFilePtr] == '{' && !isString)
                {
                    token.Value += fileData[tmpFilePtr];
                    token.tokenType = TokenType.OpenObject;
                    tmpFilePtr++; 
                    
                    break;
                }

                if (fileData[tmpFilePtr] == '}' && !isString)
                {
                    token.Value += fileData[tmpFilePtr];
                    token.tokenType = TokenType.CloseObject;
                    tmpFilePtr++;

                    break;
                }
                
                bool isWhiteSpace = IsWhiteSpace(fileData[tmpFilePtr]) && !isString;
                if (isWhiteSpace)
                {
                    break;
                }

                token.Value += fileData[tmpFilePtr];
                tmpFilePtr++;
            }
            
            filePtr = tmpFilePtr;

            // if (token.Value[0] == '*')
            // {
            //     token.tokenType = TokenType.ChunkName;
            // }
            // else
            // {
            //     token.tokenType = TokenType.ChunkData;
            // }
            
            // A valid token type has been set, therefore, we have read the token correctly.
//            return token.tokenType != TokenType.Unknown;
            return true;
        }

        private static bool ReadString(out string str)
        {
            if (Read(out Token token))
            {
                str = token.Value;
                
                if (token.tokenType != TokenType.String)
                {
                    Console.WriteLine("[ERROR]: String expected.");
                    
                    return false;
                }

                return true;
            }
            
            str = String.Empty;
            return false;
        }

        private static bool ReadRGBColour(out RGBColour colour)
        {
            //TODO: Read 3 FloatToken's instead of doing it like this.
            
            colour = new RGBColour();
            
            if (Read(out Token redToken))
            {
                if (float.TryParse(redToken.Value, out float red))
                {
                    if (red < 0.0f || red > 1.0f)
                    {
                        Console.WriteLine("[ERROR]: Red value '" + red + "' is out of range [0..1].");
                        
                        return false;
                    }

                    colour.red = red;
                }
                else
                {
                    Console.WriteLine("[ERROR]: Expected a float, got '" + redToken.Value + "'. Skipping colour.");

                    return false;
                }
            }
            
            if (Read(out Token greenToken))
            {
                if (float.TryParse(greenToken.Value, out float green))
                {
                    if (green < 0.0f || green > 1.0f)
                    {
                        Console.WriteLine("[ERROR]: Green value '" + green + "' is out of range [0..1].");
                        
                        return false;
                    }

                    colour.red = green;
                }
                else
                {
                    Console.WriteLine("[ERROR]: Expected a float, got '" + greenToken.Value + "'. Skipping colour.");

                    return false;
                }
            }

            if (Read(out Token blueToken))
            {
                if (float.TryParse(blueToken.Value, out float blue))
                {
                    if (blue < 0.0f || blue > 1.0f)
                    {
                        Console.WriteLine("[ERROR]: Blue value '" + blue + "' is out of range [0..1].");
                        
                        return false;
                    }

                    colour.red = blue;
                }
                else
                {
                    Console.WriteLine("[ERROR]: Expected a float, got '" + blueToken.Value + "'. Skipping colour.");

                    return false;
                }
            }
            
            return true;
        }
        
        public static ASEFile Import(string str)
        {
            filePtr = 0;
            fileLength = str.Length;
            fileData = str;

            ASEFile outputFile = new ASEFile();
            
            while (Read(out Token token))
            {
                switch (token.Value)
                {
                    case "*3DSMAX_ASCIIEXPORT":
                    {
                        if (Read(out Token versionNumberToken))
                        {
                            if (!int.TryParse(versionNumberToken.Value, out outputFile.exportVersion))
                            {
                                Console.WriteLine("[ERROR]: Export token is not a valid integer.");
                                
                                break;
                            }
                            
                            Console.WriteLine("Version: " + outputFile.exportVersion);
                        }
                        
                        continue;
                    }
                        break;

                    case "*COMMENT":
                    {
                        if (ReadString(out string comment))
                        {
                            outputFile.comment = comment;
                            
                            Console.WriteLine("Comment: " + outputFile.comment);
                        }
                        
                        continue;
                    }
                        break;

                    case "*SCENE":
                    {
                        if (Read(out Token openSceneToken))
                        {
                            if (openSceneToken.tokenType == TokenType.OpenObject)
                            {
                                ASEScene scene = new ASEScene();
                                outputFile.AseScene = scene;
                                
                                while (Read(out Token sceneToken))
                                {
                                    if (sceneToken.tokenType == TokenType.CloseObject)
                                    {
                                        Console.WriteLine("SCENE.SCENE_FILENAME: '" + scene.filename + "'");
                                        Console.WriteLine("SCENE.SCENE_FIRSTFRAME: " + scene.firstFrame);
                                        Console.WriteLine("SCENE.SCENE_LASTFRAME: " + scene.lastFrame);
                                        Console.WriteLine("SCENE.SCENE_FRAMESPEED: " + scene.frameSpeed);
                                        Console.WriteLine("SCENE.SCENE_TICKSPERFRAME: " + scene.ticksPerFrame);

                                        if (scene.environmentMap != null)
                                        {
                                            Console.WriteLine("SCENE_ENVMAP.MAP_NAME: '" + scene.environmentMap.MapName + "'");
                                            Console.WriteLine("SCENE.SCENE_ENVMAP.MAP_CLASS: '" + scene.environmentMap.MapClass + "'");
                                            Console.WriteLine("SCENE.SCENE_ENVMAP.MAP_SUBNO: " + scene.environmentMap.SubNo);
                                            Console.WriteLine("SCENE.SCENE_ENVMAP.MAP_AMOUNT: " + scene.environmentMap.MapAmout);
                                        }

                                        if (scene.AmbientStatic != null)
                                        {
                                            Console.WriteLine("SCENE.SCENE_AMBIENT_STATIC: " + scene.AmbientStatic);
                                        }
                                        
                                        if (scene.BackgroundStatic != null)
                                        {
                                            Console.WriteLine("SCENE.SCENE_BACKGROUND_STATIC: " + scene.BackgroundStatic);
                                        }

                                        break;
                                    }
                                    
                                    switch (sceneToken.Value)
                                    {
                                        case "*SCENE_FILENAME":
                                        {
                                            if (!ReadString(out scene.filename))
                                            {
                                                Console.WriteLine("[ERROR]: File path expected");
                                            }
                                        }
                                            break;

                                        case "*SCENE_FIRSTFRAME":
                                        {
                                            if (Read(out Token firstFrameToken))
                                            {
                                                if (!int.TryParse(firstFrameToken.Value, out scene.firstFrame))
                                                {
                                                    Console.WriteLine("[ERROR]: First frame expected");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("[ERROR]: First frame expected");
                                            }
                                        }
                                            break;
                                        
                                        case "*SCENE_LASTFRAME":
                                        {
                                            if (Read(out Token lastFrameToken))
                                            {
                                                if (!int.TryParse(lastFrameToken.Value, out scene.lastFrame))
                                                {
                                                    Console.WriteLine("[ERROR]: Last frame expected");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("[ERROR]: Last frame expected");
                                            }
                                        }
                                            break;
                                        
                                        case "*SCENE_FRAMESPEED":
                                        {
                                            if (Read(out Token frameSpeedToken))
                                            {
                                                if (!float.TryParse(frameSpeedToken.Value, out scene.frameSpeed))
                                                {
                                                    Console.WriteLine("[ERROR]: Frame speed expected");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("[ERROR]: Frame speed expected");
                                            }
                                        }
                                            break;
                                        
                                        case "*SCENE_TICKSPERFRAME":
                                        {
                                            if (Read(out Token ticksPerFrameToken))
                                            {
                                                if (!int.TryParse(ticksPerFrameToken.Value, out scene.ticksPerFrame))
                                                {
                                                    Console.WriteLine("[ERROR]: Ticks per frame expected");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("[ERROR]: Ticks per frame expected");
                                            }
                                        }
                                            break;
                                        
                                        case "*SCENE_ENVMAP":
                                        {
                                            if (Read(out Token openEnvMapToken))
                                            {
                                                scene.environmentMap = new ASEEnvironmentMap();

                                                if (openEnvMapToken.tokenType != TokenType.OpenObject)
                                                {
                                                    Console.WriteLine("Expected an '{'.");
                                                    
                                                    break;
                                                }
                                                    
                                                while (Read(out Token envMapToken))
                                                {
                                                    if (envMapToken.tokenType == TokenType.CloseObject)
                                                    {
                                                        break;
                                                    }

                                                    switch (envMapToken.Value)
                                                    {
                                                        case "*MAP_NAME":
                                                        {
                                                            if (!ReadString(out scene.environmentMap.MapName))
                                                            {
                                                                Console.WriteLine("[ERROR]: Map name expected");
                                                            }
                                                        }
                                                            break;
                                                        
                                                        case "*MAP_CLASS":
                                                        {
                                                            if (!ReadString(out scene.environmentMap.MapClass))
                                                            {
                                                                Console.WriteLine("[ERROR]: Map class expected");
                                                            }
                                                        }
                                                            break;
                                                        
                                                        case "*MAP_SUBNO":
                                                        {
                                                            if (Read(out Token subNoToken))
                                                            {
                                                                if (!int.TryParse(subNoToken.Value, out scene.environmentMap.SubNo))
                                                                {
                                                                    Console.WriteLine("[ERROR]: Sub no. expected");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine("[ERROR]: Sub no. expected");
                                                            }
                                                        }
                                                            break;
                                                        
                                                        case "*MAP_AMOUNT":
                                                        {
                                                            if (Read(out Token amountToken))
                                                            {
                                                                if (!float.TryParse(amountToken.Value, out scene.environmentMap.MapAmout))
                                                                {
                                                                    Console.WriteLine("[ERROR]: Map Amount expected");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine("[ERROR]: Map Amount expected");
                                                            }
                                                        }
                                                            break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("[ERROR]: Open Block Token expected");
                                            }
                                        }
                                            break;

                                        case "*SCENE_AMBIENT_STATIC":
                                        {
                                            if (ReadRGBColour(out RGBColour colourToken))
                                            {
                                                scene.AmbientStatic = new ASEColor()
                                                {
                                                    red = colourToken.red,
                                                    green = colourToken.green,
                                                    blue = colourToken.blue
                                                };
                                            }
                                        }
                                            break;

                                        case "*SCENE_BACKGROUND_STATIC":
                                        {
                                            if (ReadRGBColour(out RGBColour colourToken))
                                            {
                                                scene.BackgroundStatic = new ASEColor()
                                                {
                                                    red = colourToken.red,
                                                    green = colourToken.green,
                                                    blue = colourToken.blue
                                                };
                                            }
                                        }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                        break;

                    case "*MATERIAL_LIST":
                    {
                        //TODO: We can remove some indendation here
                        if (Read(out Token openMaterialListToken))
                        {
                            if (openMaterialListToken.tokenType != TokenType.OpenObject)
                            {
                                Console.WriteLine("Expected an '{'.");

                                break;
                            }

                            int materialCount = 0;
                            
// Read the first token. It must be MATERIAL_COUNT

                            if (Read(out Token materialListToken))
                            {
                                if (materialListToken.tokenType == TokenType.CloseObject)
                                {
                                    break;
                                }

                                if (materialListToken.Value == "*MATERIAL_COUNT")
                                {
                                    if (Read(out Token materialCountToken))
                                    {
                                        if (int.TryParse(materialCountToken.Value, out materialCount))
                                        {
                                            outputFile.MaterialList = new ASEMaterial[materialCount];
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid integer value '" + materialListToken.Value);

                                            break;
                                        }
                                    }

                                }
                                else if (materialListToken.Value == "*MATERIAL")
                                {

                                }
                                else
                                {
                                    Console.WriteLine("[ERROR] Unknown chunk name '" + materialListToken.Value + "'.");
                                }

                            }
                        }

                        Console.WriteLine("MATERIAL_LIST");
                    }
                        break;
                }
                
               // Console.Write(currentToken.Value);
            }
            
            return null;
        }
    }
}