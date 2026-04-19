SET FOREIGN_KEY_CHECKS = 0;

DELETE FROM Projects;
DELETE FROM Users;
DELETE FROM RegistrationRequests;

SET FOREIGN_KEY_CHECKS = 1;


INSERT INTO Users
(Name, Email, PasswordHash, Role, Batch, RegistrationNumber, Department, ResearchAreaId, IsActive)
VALUES
('Admin User', 'admin@gmail.com', '$2a$11$TCNwbQxgVTxxr0S5DMEFueAQIndOOCaP1oNkcUEfVraTdKXAb7icO', 2, NULL, NULL, NULL, NULL, 1);


INSERT INTO Users
(Name, Email, PasswordHash, Role, Batch, RegistrationNumber, Department, ResearchAreaId, IsActive)
VALUES
('Student A', 'sa@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.1', 'SE241001', NULL, NULL, 1),
('Student B', 'sb@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.1', 'SE241002', NULL, NULL, 1),
('Student C', 'sc@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.1', 'SE241003', NULL, NULL, 1),
('Student D', 'sd@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.1', 'SE241004', NULL, NULL, 1);

INSERT INTO Users
(Name, Email, PasswordHash, Role, Batch, RegistrationNumber, Department, ResearchAreaId, IsActive)
VALUES
('Student E', 'se@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.2', 'SE242001', NULL, NULL, 1),
('Student F', 'sf@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.2', 'SE242002', NULL, NULL, 1),
('Student G', 'sg@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.2', 'SE242003', NULL, NULL, 1),
('Student H', 'sh@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.2', 'SE242004', NULL, NULL, 1);


INSERT INTO Users
(Name, Email, PasswordHash, Role, Batch, RegistrationNumber, Department, ResearchAreaId, IsActive)
VALUES
('Student I', 'si@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '23.2', 'SE232001', NULL, NULL, 1),
('Student J', 'sj@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '23.2', 'SE232002', NULL, NULL, 1),
('Student K', 'sk@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '23.2', 'SE232003', NULL, NULL, 1);


INSERT INTO Users
(Name, Email, PasswordHash, Role, Batch, RegistrationNumber, Department, ResearchAreaId, IsActive)
VALUES
('Dr. Silva', 'sup1@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 1, NULL, NULL, 'Computing', 1, 1),
('Dr. Perera', 'sup2@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 1, NULL, NULL, 'IT', 2, 1),
('Dr. Fernando', 'sup3@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 1, NULL, NULL, 'Cyber Security', 3, 1),
('Dr. Jayasuriya', 'sup4@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 1, NULL, NULL, 'Cloud Computing', 4, 1),
('Dr. Wijesinghe', 'sup5@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 1, NULL, NULL, 'Data Science', 5, 1);


INSERT INTO Projects
(Title, Description, TechStack, ResearchAreaId, StudentId, SupervisorId, Status, CreatedAt)
VALUES
('AI Chatbot System',
 'A smart chatbot to answer university-related student queries using natural language processing.',
 'C#, AI, NLP, .NET',
 1, 2, NULL, 0, NOW()),

('E-Commerce Web Platform',
 'A full-stack online shopping system with product browsing, cart, checkout, and admin panel.',
 'ASP.NET, MySQL, Bootstrap, JavaScript',
 2, 3, NULL, 0, NOW()),

('Cyber Threat Monitoring Dashboard',
 'A dashboard to detect and visualize suspicious network activity using rule-based analysis.',
 'Python, Security, Dashboard, SQL',
 3, 4, NULL, 0, NOW()),

('Cloud File Storage Manager',
 'A secure cloud-based file storage and sharing system with access control and versioning.',
 'Cloud, .NET, Azure, SQL',
 4, 5, NULL, 0, NOW()),

('Student Performance Prediction',
 'A machine learning solution to predict academic performance from attendance and assessment data.',
 'Python, ML, Data Science, Pandas',
 5, 6, NULL, 0, NOW()),

('Mobile Learning App',
 'A mobile application that helps students access notes, quizzes, and reminders on the go.',
 'Flutter, Firebase, Dart',
 6, 7, NULL, 0, NOW()),

('IoT Smart Parking System',
 'An IoT-based smart parking allocation and monitoring system using sensors and live status updates.',
 'ESP32, IoT, C++, Sensors',
 8, 8, NULL, 0, NOW()),

('Online Examination Platform',
 'A secure online examination system with timer, question randomization, and result generation.',
 'ASP.NET, SQL, JavaScript',
 2, 9, NULL, 0, NOW()),

('Machine Learning Course Recommender',
 'A recommendation engine that suggests personalized learning paths for students.',
 'Python, ML, Recommendation System',
 5, 10, NULL, 0, NOW()),

('Secure Password Vault',
 'A password manager for storing encrypted credentials with role-based access.',
 'C#, Encryption, Security',
 3, 11, NULL, 0, NOW()),

('Smart Attendance Tracker',
 'A digital attendance system with analytics and exportable reports for lecturers and admins.',
 'ASP.NET Core, MySQL, Charts',
 2, 12, NULL, 0, NOW());


INSERT INTO RegistrationRequests
(Name, Email, PasswordHash, Role, Batch, RegistrationNumber, Department, ResearchAreaId, Status, RequestedAt)
VALUES
('Pending Student', 'pendingstudent@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 0, '24.1', 'SE241099', NULL, NULL, 'Pending', NOW()),
('Pending Supervisor', 'pendingsup@gmail.com', '$2a$11$hkoHb4/wfrvM51Ul3A0aw.c1BnwdhnylYFIXbHTS7GBup4GwEG0Gi', 1, NULL, NULL, 'Computing', 1, 'Pending', NOW());