using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Api.Infrastructure.Seed;

/// <summary>
/// Runs once at startup, right after migrations. Safe to run every time the app
/// starts - each section checks for its own existing data before inserting, so
/// nothing is duplicated and no section is skipped just because another table
/// already has rows (e.g. a real user signing up before the seeder runs).
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var jobsDb = scope.ServiceProvider.GetRequiredService<JobsDbContext>();

        // ---- Categories ----
        var categories = await SeedCategoriesAsync(jobsDb);

        // ---- Employers + Companies ----
        var companies = await SeedCompaniesAsync(jobsDb, userManager);

        // ---- Jobs (spread across the 3 companies and 4 categories) ----
        await SeedJobsAsync(jobsDb, companies, categories);

        // ---- Candidates + Candidate Profiles ----
        await SeedCandidatesAsync(jobsDb, userManager);
    }

    private static async Task<List<Category>> SeedCategoriesAsync(JobsDbContext jobsDb)
    {
        var wanted = new[]
        {
            new Category { Name = "Development", Slug = "development", DisplayOrder = 1, IsFeatured = true },
            new Category { Name = "UI/UX Design", Slug = "ui-ux-design", DisplayOrder = 2, IsFeatured = true },
            new Category { Name = "Marketing", Slug = "marketing", DisplayOrder = 3, IsFeatured = true },
            new Category { Name = "Customer Service", Slug = "customer-service", DisplayOrder = 4, IsFeatured = true },
        };

        var existingSlugs = await jobsDb.Categories
            .Select(c => c.Slug)
            .ToListAsync();

        var toAdd = wanted.Where(c => !existingSlugs.Contains(c.Slug)).ToList();
        if (toAdd.Count > 0)
        {
            jobsDb.Categories.AddRange(toAdd);
            await jobsDb.SaveChangesAsync();
        }

        // Return the full set (existing + newly added) so downstream code has Ids for all of them.
        return await jobsDb.Categories
            .Where(c => wanted.Select(w => w.Slug).Contains(c.Slug))
            .ToListAsync();
    }

    private static async Task<List<Company>> SeedCompaniesAsync(JobsDbContext jobsDb, UserManager<AppUser> userManager)
    {
        var employerSeeds = new[]
        {
            ("employer1@jobi.test", "Ali", "Mammadov", "NovaTech Solutions", "novatech-solutions"),
            ("employer2@jobi.test", "Leyla", "Huseynova", "Baku Digital Agency", "baku-digital-agency"),
            ("employer3@jobi.test", "Rashad", "Aliyev", "Caspian Retail Group", "caspian-retail-group"),
        };

        var existingSlugs = await jobsDb.Companies
            .Select(c => c.Slug)
            .ToListAsync();

        foreach (var (email, firstName, lastName, companyName, slug) in employerSeeds)
        {
            if (existingSlugs.Contains(slug))
                continue; // this employer/company already seeded

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = UserRole.Employer,
                    EmailConfirmed = true,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                };
                await userManager.CreateAsync(user, "Seed@12345");
            }

            jobsDb.Companies.Add(new Company
            {
                OwnerUserId = user.Id,
                Name = companyName,
                Slug = slug,
                Description = $"{companyName} is a growing company based in Baku, Azerbaijan.",
                WebsiteUrl = $"https://{slug}.example.com"
            });
        }
        await jobsDb.SaveChangesAsync();

        return await jobsDb.Companies
            .Where(c => employerSeeds.Select(e => e.Item5).Contains(c.Slug))
            .ToListAsync();
    }

    private static async Task SeedJobsAsync(JobsDbContext jobsDb, List<Company> companies, List<Category> categories)
    {
        Company CompanyBySlug(string slug) => companies.First(c => c.Slug == slug);
        Category CategoryBySlug(string slug) => categories.First(c => c.Slug == slug);

        var jobSeeds = new[]
        {
            ("Senior Backend Developer", "novatech-solutions", "development", JobType.FullTime, ExperienceLevel.Senior),
            ("Frontend Developer", "novatech-solutions", "development", JobType.FullTime, ExperienceLevel.Intermediate),
            ("Junior QA Engineer", "novatech-solutions", "development", JobType.FullTime, ExperienceLevel.EntryLevel),
            ("Product Designer", "baku-digital-agency", "ui-ux-design", JobType.FullTime, ExperienceLevel.Intermediate),
            ("UX Researcher", "baku-digital-agency", "ui-ux-design", JobType.PartTime, ExperienceLevel.Senior),
            ("Digital Marketing Specialist", "baku-digital-agency", "marketing", JobType.FullTime, ExperienceLevel.Intermediate),
            ("SEO Executive", "baku-digital-agency", "marketing", JobType.Remote, ExperienceLevel.EntryLevel),
            ("Customer Support Representative", "caspian-retail-group", "customer-service", JobType.FullTime, ExperienceLevel.EntryLevel),
            ("Customer Success Manager", "caspian-retail-group", "customer-service", JobType.FullTime, ExperienceLevel.Senior),
        };

        var existingSlugs = await jobsDb.Jobs
            .Select(j => j.Slug)
            .ToListAsync();

        var toAdd = new List<Job>();
        for (int i = 0; i < jobSeeds.Length; i++)
        {
            var (title, companySlug, categorySlug, jobType, level) = jobSeeds[i];
            var slug = GenerateSlug(title);
            if (existingSlugs.Contains(slug))
                continue;

            toAdd.Add(new Job
            {
                Title = title,
                Slug = slug,
                Description = $"We are looking for a {title} to join our team. Great growth opportunity in a fast-moving company.",
                CategoryId = CategoryBySlug(categorySlug).Id,
                CompanyId = CompanyBySlug(companySlug).Id,
                Location = "Baku, Azerbaijan",
                JobType = jobType,
                ExperienceLevel = level,
                SalaryMin = 800,
                SalaryMax = 2500,
                VacancyCount = 1,
                ApplicationDeadline = DateTimeOffset.UtcNow.AddMonths(2),
                Status = JobStatus.Active,
                IsFeatured = i % 3 == 0
            });
        }

        if (toAdd.Count > 0)
        {
            jobsDb.Jobs.AddRange(toAdd);
            await jobsDb.SaveChangesAsync();
        }
    }

    private static async Task SeedCandidatesAsync(JobsDbContext jobsDb, UserManager<AppUser> userManager)
    {
        var candidateSeeds = new[]
        {
            ("candidate1@jobi.test", "Nigar", "Qasimova", "Frontend Developer", "React, TypeScript, CSS", ExperienceLevel.Intermediate),
            ("candidate2@jobi.test", "Tural", "Ismayilov", "UI/UX Designer", "Figma, Adobe XD, Prototyping", ExperienceLevel.Senior),
            ("candidate3@jobi.test", "Aygun", "Rzayeva", "Marketing Coordinator", "SEO, Content Writing, Analytics", ExperienceLevel.EntryLevel),
        };

        var existingSlugs = await jobsDb.CandidateProfiles
            .Select(p => p.Slug)
            .ToListAsync();

        foreach (var (email, firstName, lastName, headline, skills, level) in candidateSeeds)
        {
            var slug = GenerateSlug($"{firstName} {lastName}");
            if (existingSlugs.Contains(slug))
                continue; // already seeded

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = UserRole.Candidate,
                    EmailConfirmed = true,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                };
                await userManager.CreateAsync(user, "Seed@12345");
            }

            jobsDb.CandidateProfiles.Add(new CandidateProfile
            {
                OwnerUserId = user.Id,
                Slug = slug,
                FullName = $"{firstName} {lastName}",
                Headline = headline,
                Bio = $"{headline} based in Baku with hands-on experience delivering real projects.",
                Location = "Baku, Azerbaijan",
                Skills = skills,
                ExperienceLevel = level,
                IsAvailableForWork = true
            });
        }
        await jobsDb.SaveChangesAsync();
    }

    private static string GenerateSlug(string input)
    {
        var slug = input.Trim().ToLowerInvariant().Replace(" ", "-");
        return System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9-]", "");
    }
}