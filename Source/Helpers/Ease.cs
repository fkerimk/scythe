internal static class Ease {

    public static float OutBack(float t) {

        const float s = 1.70158f;

        return (t -= 1) * t * ((s + 1) * t + s) + 1;
    }

    public static float InCubic(float t) => t * t * t;
}