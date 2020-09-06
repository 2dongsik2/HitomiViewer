using System;

namespace HitomiViewer.Structs
{
    public class Tag
    {
        public Types types { get; set; }
        public string name { get; set; }
        public string full { get; set; }
        public enum Types
        {
            female,
            male,
            tag,
            type,
            artist,
            character,
            group,
            series,
            language,
            none
        }
        public bool Hitomi = true;

        public void FullNameParse(string value)
        {
            if (value.Contains(":"))
            {
                this.full = value;
                this.name = NameParse(this.full);
            }
            else
            {
                this.full = this.types.ToString() + value;
                this.name = value;
            }
        }
        public void Check()
        {
            Hitomi = isHitomi(full);
        }

        public static Tag Parse(string value)
        {
            if (!value.Contains(":"))
                value = "tag:" + value;
            Tag tag = new Tag();
            tag.types = ParseTypes(value);
            tag.Hitomi = isHitomi(value);
            tag.name = value.Split(':')[1];
            tag.full = value;
            return tag;
        }
        public static string NameParse(string value)
        {
            return value.Split(':')[1];
        }
        public static Types ParseTypes(string value)
        {
            if (value.Contains(":"))
            {
                Types type;
                bool err = Enum.TryParse<Types> (value.Split(':')[0], out type);
                if (err)
                    return type;
                else
                {
                    switch (value.Split(':')[0])
                    {
                        case "남":
                            return Types.male;
                        case "여":
                            return Types.female;
                        case "캐릭":
                            return Types.character;
                        case "태그":
                            return Types.tag;
                        case "종류":
                            return Types.type;
                        default:
                            return Types.none;
                    }
                }
            }
            else
                return Types.tag;
        }
        public static bool isHitomi(string value)
        {
            if (value == null) return false;
            if (value.Contains(":"))
            {
                Types type;
                return Enum.TryParse<Types>(value.Split(':')[0], out type);
            }
            else
                return false;
        }
    }
}
