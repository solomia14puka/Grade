using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GradeApp.Models;

namespace GradeApp.Data
{
    public class GradeAppContext : DbContext
    {
        public GradeAppContext (DbContextOptions<GradeAppContext> options)
            : base(options)
        {
        }

        public DbSet<GradeApp.Models.Faculty> Faculty { get; set; } = default!;
        public DbSet<GradeApp.Models.Department> Department { get; set; } = default!;
        public DbSet<GradeApp.Models.Professor> Professor { get; set; } = default!;
        public DbSet<GradeApp.Models.Student> Student { get; set; } = default!;
        public DbSet<GradeApp.Models.Subject> Subject { get; set; } = default!;
        public DbSet<GradeApp.Models.Grade> Grade { get; set; } = default!;
    }
}
