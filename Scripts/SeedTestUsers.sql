-- Тестовые пользователи для экрана входа (пароль: 1234)
USE AgroCompany;
GO

IF NOT EXISTS (SELECT 1 FROM UserRole WHERE RoleName = N'Менеджер по продажам')
    INSERT INTO UserRole (RoleName) VALUES
        (N'Менеджер по продажам'),
        (N'Руководитель склада'),
        (N'Генеральный директор'),
        (N'Коммерческий директор');
GO

DECLARE @ManagerRole TINYINT = (SELECT RoleID FROM UserRole WHERE RoleName = N'Менеджер по продажам');
DECLARE @WarehouseRole TINYINT = (SELECT RoleID FROM UserRole WHERE RoleName = N'Руководитель склада');
DECLARE @CeoRole TINYINT = (SELECT RoleID FROM UserRole WHERE RoleName = N'Генеральный директор');
DECLARE @ComRole TINYINT = (SELECT RoleID FROM UserRole WHERE RoleName = N'Коммерческий директор');

IF NOT EXISTS (SELECT 1 FROM UserAccount WHERE Login = 'manager1')
BEGIN
    INSERT INTO UserAccount (RoleID, Login, PasswordHash, IsBlocked) VALUES
        (@ManagerRole, 'manager1', '1234', 0),
        (@WarehouseRole, 'warehouse1', '1234', 0),
        (@CeoRole, 'director', '1234', 0),
        (@ComRole, 'comdirector', '1234', 0),
        (@ManagerRole, 'blocked_user', '1234', 1);

    INSERT INTO Employee (AccountID, LastName, FirstName, MiddleName, PhoneNumber)
    SELECT AccountID, LastName, FirstName, MiddleName, '+7 (900) 000-00-00'
    FROM (
        VALUES
            ('manager1', N'Иванов', N'Иван', N'Иванович'),
            ('warehouse1', N'Петров', N'Пётр', N'Петрович'),
            ('director', N'Сидоров', N'Сергей', N'Сергеевич'),
            ('comdirector', N'Козлов', N'Кирилл', N'Кириллович'),
            ('blocked_user', N'Блок', N'Борис', N'Борисович')
    ) AS src(Login, LastName, FirstName, MiddleName)
    INNER JOIN UserAccount ua ON ua.Login = src.Login;
END
GO
