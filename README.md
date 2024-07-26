# ArianNovin Web Application

Welcome to the ArianNovin Web Application! This project is developed as a demonstration of various web development techniques and practices, including user management, post management, course management, and more.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Setup Instructions](#setup-instructions)
- [Usage](#usage)
- [REST API](#rest-api)
- [Contributing](#contributing)

## Features

- User Authentication and Authorization
- Course Management
  - Create, Edit, and Delete Courses
  - Enroll in Courses
- Post Management
  - Create, Edit, and Delete Posts
  - Comment and Reply System
- Responsive Design
- REST API

## Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- Microsoft Identity
- Bootstrap
- jQuery

## Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Hr-Panahi/ArianNovinProj.git
   cd ArianNovinProj
   
2. **Setup the database:**
- Ensure you have SQL Server installed and running.
- Update the connection string in appsettings.json to match your SQL Server instance.

3. **Apply migrations:**
   ```bash
   dotnet ef database update

4. **Run the application:**
   ```bash
   dotnet run

5. **Seed the database:**
- The application will seed the database with initial data (admin user and roles) on first run.

## Usage
- Home Page: Provides an overview of the application with links to the main features.
- Courses: Admins can create, edit, and delete courses. Users can enroll in available courses.
- Posts: Users can create, edit, and delete posts. They can also comment on posts and reply to comments.

## REST API
The application includes a REST API for managing posts and courses.
### Endpoints
#### Posts
- GET /api/ApiPost - Retrieves all posts
- GET /api/ApiPost/{id} - Retrieves a specific post by ID
- POST /api/ApiPost - Creates a new post
- PUT /api/ApiPost/{id} - Updates an existing post
- DELETE /api/ApiPost/{id} - Deletes a post
#### Courses
- GET /api/ApiCourse - Retrieves all courses
- GET /api/ApiCourse/{id} - Retrieves a specific course by ID
- POST /api/ApiCourse - Creates a new course
- PUT /api/ApiCourse/{id} - Updates an existing course
- DELETE /api/ApiCourse/{id} - Deletes a course

#### Contributing
Contributions are welcome! Please fork the repository and create a pull request with your changes.
1. Fork the repository.
2. Create a new branch: git checkout -b my-feature-branch
3. Make your changes and commit them: git commit -m 'Add some feature'
4. Push to the branch: git push origin my-feature-branch
5. Create a pull request.
