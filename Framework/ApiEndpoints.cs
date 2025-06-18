namespace Bird.Framework
{
    /// <summary>
    /// Contains all API endpoints used in tests.
    /// This centralizes endpoint management and provides type safety.
    /// </summary>
    public static class ApiEndpoints
    {
        /// <summary>
        /// User management endpoints
        /// </summary>
        public static class Users
        {
            private const string Base = "users";
            
            /// <summary>
            /// GET /users
            /// </summary>
            public const string GetAll = Base;
            
            /// <summary>
            /// GET /users/{id}
            /// </summary>
            public static string GetById(string id) => $"{Base}/{id}";
            
            /// <summary>
            /// POST /users
            /// </summary>
            public const string Create = Base;
            
            /// <summary>
            /// PUT /users/{id}
            /// </summary>
            public static string Update(string id) => $"{Base}/{id}";
            
            /// <summary>
            /// DELETE /users/{id}
            /// </summary>
            public static string Delete(string id) => $"{Base}/{id}";
        }

        /// <summary>
        /// Authentication endpoints
        /// </summary>
        public static class Auth
        {
            private const string Base = "auth";
            
            /// <summary>
            /// POST /auth/login
            /// </summary>
            public const string Login = $"{Base}/login";
            
            /// <summary>
            /// POST /auth/refresh
            /// </summary>
            public const string RefreshToken = $"{Base}/refresh";
            
            /// <summary>
            /// POST /auth/logout
            /// </summary>
            public const string Logout = $"{Base}/logout";
        }

        /// <summary>
        /// Resource management endpoints
        /// </summary>
        public static class Resources
        {
            private const string Base = "resources";
            
            /// <summary>
            /// GET /resources
            /// </summary>
            public const string GetAll = Base;
            
            /// <summary>
            /// GET /resources/{id}
            /// </summary>
            public static string GetById(string id) => $"{Base}/{id}";
            
            /// <summary>
            /// POST /resources
            /// </summary>
            public const string Create = Base;
            
            /// <summary>
            /// PUT /resources/{id}
            /// </summary>
            public static string Update(string id) => $"{Base}/{id}";
            
            /// <summary>
            /// DELETE /resources/{id}
            /// </summary>
            public static string Delete(string id) => $"{Base}/{id}";
        }
    }
} 