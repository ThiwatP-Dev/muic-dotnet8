namespace KeystoneLibrary.Providers
{
    public class ColorProvider
    {
        private readonly static string[] colors = new string[] { "#FF2929", "#FFC40F", "#3498DB", "#3DCE39", "#9B59B6", 
                                                                 "#FF8200", "#7F8C8D", "#D97F63", "#009999", "#FF1290", 
                                                                 "#003366", "#66FFFF", "#636E72", "#636E72", "#636E72" };

        public static string ColorCodeGenerator(int index)
        {
            if (index >= colors.Length)
            {
                return "#CCCCCC";
            }
            else
            {
                return colors[index]; 
            }         
        }
    }
}