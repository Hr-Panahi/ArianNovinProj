﻿using ArianNovinWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ArianNovinWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        //Configuring DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
    }
}
