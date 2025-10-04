
CREATE TABLE Roles (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(255),
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Gender VARCHAR(10) CHECK (Gender IN ('Male', 'Female')),
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

--один к одному
CREATE TABLE UserProfiles (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL UNIQUE,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    BirthDate DATE NULL,
    Phone VARCHAR(20) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

--один ко многим
CREATE TABLE UserAddresses (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Street VARCHAR(255) NOT NULL,
    City VARCHAR(100) NOT NULL,
    Country VARCHAR(100) NOT NULL,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- многие-ко-многим
CREATE TABLE UserRoles (
    UserId UUID NOT NULL,
    RoleId INT NOT NULL,
    AssignedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

INSERT INTO Roles (Name, Description) VALUES 
('Admin', 'Administrator with full access'),
('User', 'Regular user');

INSERT INTO Users (Username, Email, PasswordHash, Gender) VALUES 
('admin', 'admin@company.com', 'hashed_password_1', 'Male'),
('alice_s', 'alice@mail.com', 'hashed_password_2', 'Female');

INSERT INTO UserProfiles (UserId, FirstName, LastName, BirthDate, Phone) VALUES 
((SELECT Id FROM Users WHERE Username = 'admin'), 'John', 'Doe', '1985-05-15', '+1234567890'),
((SELECT Id FROM Users WHERE Username = 'alice_s'), 'Alice', 'Smith', '1990-08-22', '+1234567891');

INSERT INTO UserAddresses (UserId, Street, City, Country) VALUES 
((SELECT Id FROM Users WHERE Username = 'admin'), 'Parina', 'Kazan', 'Russia'),
((SELECT Id FROM Users WHERE Username = 'admin'), 'Pobedy', 'Moscow', 'Russia'),
((SELECT Id FROM Users WHERE Username = 'alice_s'),'Sreet', 'Elabuga', 'Russia');

INSERT INTO UserRoles (UserId, RoleId) VALUES 
((SELECT Id FROM Users WHERE Username = 'admin'), 1),
((SELECT Id FROM Users WHERE Username = 'alice_s'), 2);

-- пользователи с максимальной и минимальной датой регистрации
SELECT 
    'Earliest Registration' AS Type,
    u.Username,
    u.Email,
    up.FirstName,
    up.LastName,
    u.CreatedDate
FROM Users u
INNER JOIN UserProfiles up ON u.Id = up.UserId
WHERE u.CreatedDate = (SELECT MIN(CreatedDate) FROM Users)

UNION ALL

SELECT 
    'Latest Registration' AS Type,
    u.Username,
    u.Email,
    up.FirstName,
    up.LastName,
    u.CreatedDate
FROM Users u
INNER JOIN UserProfiles up ON u.Id = up.UserId
WHERE u.CreatedDate = (SELECT MAX(CreatedDate) FROM Users);

-- количество мужчин и женщин
SELECT 
    Gender,
    COUNT(Id) AS UserCount
FROM Users
WHERE Gender IS NOT NULL
GROUP BY Gender
ORDER BY UserCount DESC;

-- статистика по ролям (сколько пользователей в каждой роли)
SELECT 
    r.Name AS RoleName,
    r.Description,
    COUNT(ur.UserId) AS UserCount
FROM Roles r
LEFT JOIN UserRoles ur ON r.Id = ur.RoleId
GROUP BY r.Id
ORDER BY UserCount DESC;