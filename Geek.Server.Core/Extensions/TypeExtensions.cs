namespace System
{
    public static class TypeExtensions
    {
        public static bool IsImplWithInterface(this Type self, Type target)
        {
            return self.GetInterface(target.FullName) != null && !self.IsInterface && !self.IsAbstract;
        }

    }
}
