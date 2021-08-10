namespace ASEParser
{
    public class ASEFile
    {
        public int exportVersion;
        public string comment;
        public ASEScene AseScene;
        public ASEMaterial[] MaterialList;
    }
    
    public class ASEMaterial
    {
        public string Name;
        public string Class;
        public ASEColor Ambient;
        public ASEColor Diffuse;
        public ASEColor Specular;
        public float Shine;
        public float ShineStrength;
        public float Transparency;
        public float WireSize;
        public string Shading; //TODO: Should be an enum
        public float XpFallOff;
        public float SelfIllumination;
        public string FallOff; //TODO: Should be an enum
        public string XpType; //TODO: Should be an enum
        public AseMapDiffuse MapDiffuse;
    }

    public class AseMapDiffuse
    {
        public string Name;
        public string Class;
        public int MapNo;
        public float Amount;
        public string Bitmap;
        public string MapType; // TODO: Should be an enum
        public float uvwUOffset;
        public float uvwVOffset;
        public float uvwUTiling;
        public float uvwVTiling;
        public float uvwAngle;
        public float uvwBlur;
        public float uvwBlufOffset;
        public float uvwNouseAmount;
        public float uvwNoiseSize;
        public float uvwNoiseLevel;
        public float uvwNoisePhase;
        public string BitMapFilter; //TODO: Should be an enum
    }
        
    public class ASEScene
    {
        public string filename;
        public int firstFrame;
        public int lastFrame;
        public float frameSpeed;
        public int ticksPerFrame;
        // Appears to be optional
        public ASEEnvironmentMap environmentMap;
//Appears to be optional
        public ASEColor BackgroundStatic;
        public ASEColor AmbientStatic;
    }

    public class ASEColor
    {
        public float red;
        public float green;
        public float blue;

        public override string ToString()
        {
            return "(" + red + ", " + green + ", " + blue + ")";
        }
    }
	    
    public class ASEEnvironmentMap
    {
        public string MapName;
        public string MapClass;
        public int SubNo;
        public float MapAmout;
    }
}