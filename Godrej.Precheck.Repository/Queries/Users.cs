using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Queries
{
    public static class Users
    {
        #region User Queries

        // Get user by email 
        public static readonly string GET_USER_BY_EMAIL = @"
            SELECT id, email, username, userid 
            FROM tbl_users 
            WHERE email = @Email and isactive=1 ";

        // Get user by UserName
        public static readonly string GET_USER_BY_USERNAME = @"
            SELECT id, email, username, userid 
            FROM tbl_users 
            WHERE username = @UserName and isactive=1 ";

        public static readonly string GET_USER_BY_USERID = @"
            SELECT id, email, username, securityquestionid, securityanswer, userid, approvedby
            FROM tbl_users 
            WHERE userid = @UserId and isactive = 1";

        // Insert new user
        public static readonly string INSERT_USER_QUERY = @"
            INSERT INTO tbl_users (email, userid, passwordhash, username, securitystamp,userroleid,plantid ,createddate, isactive,departmentid,securityquestionid,securityanswer, approvedby)
            OUTPUT INSERTED.id
            VALUES (@Email, @userId, @PasswordHash, @Username, @SecurityStamp, @UserRoleId, @PlantId, @CreatedDate, @IsActive,@departmentid,@securityquestionid,@securityanswer, 0)";

        public static readonly string UPDATE_USER_QUERY = @"
            UPDATE  tbl_users 
            SET passwordhash = @PasswordHash, securitystamp = @SecurityStamp, securityanswer = @SecurityAnswer, modifieddate = GETDATE() 
            WHERE userid = @UserId";

        public static readonly string APPROVE_USER_QUERY = @"
            UPDATE tbl_users
            SET approvedby = 1,
                modifiedby = @ModifiedBy,
                modifieddate = GETDATE()
            WHERE id = @Id";

        public static readonly string GET_PENDING_USERS_QUERY = @"
            SELECT id, email, username, userid, isactive, departmentid, plantid, userroleid, createddate
            FROM tbl_users 
            WHERE approvedby = 0";
        #endregion

        #region Refresh Token Queries

        // Insert new refresh token
        public static readonly string INSERT_REFRESH_TOKEN_QUERY = @"
            INSERT INTO tbl_userrefreshtokens (userid, token, expirydate, lastactivity, isactive)
            OUTPUT INSERTED.id
            VALUES (@userid, @token, @expirydate, @lastactivity, @isactive)";

        // Get refresh token by token value
        public static readonly string GET_REFRESH_TOKEN_BY_TOKEN = @"
            SELECT id, userid, token, expirydate, lastactivity, revokedat, createdby, createddate, isactive 
            FROM tbl_userrefreshtokens 
            WHERE token = @Token";

        // Revoke refresh token by ID
        public static readonly string REVOKE_REFRESH_TOKEN_BY_ID = @"
            UPDATE tbl_userrefreshtokens 
            SET revokedat = @RevokedAt, modifieddate = @ModifiedDate, isactive = 0 
            WHERE id = @Id";

        #endregion

        #region Helper Queries

        // Get user by ID
        public static readonly string GET_USER_BY_ID_QUERY = @"
            SELECT * 
            FROM tbl_users 
            WHERE id = @Id";

        // Update refresh token
        public static readonly string UPDATE_REFRESH_TOKEN_QUERY = @"
            UPDATE tbl_userrefreshtokens 
            SET token = @Token, expirydate = @ExpiryDate, modifieddate = @ModifiedDate 
            WHERE id = @Id";

        // Check if username exists
        public static readonly string CHECK_USERNAME_EXISTS_QUERY = @"
          SELECT u.id, u.username, u.userid, u.passwordhash, u.securitystamp,u.userroleid,u.plantid,u.email, ur.role,u.departmentid , dept.name as departmentname, u.approvedBy as ApprovedBy, u.isactive
             FROM tbl_users u
             inner join tbl_userroles ur on u.userroleid = ur.id
             inner join tbl_department dept on u.departmentid = dept.id
             WHERE u.userid =  @userid AND u.isactive = 1";

        // Check if email exists
        public static readonly string CHECK_EMAIL_EXISTS_QUERY = @"
            SELECT id, email, username, userid, plantid, createdby, createddate, modifiedby, modifieddate, isactive, passwordhash, securitystamp, lastloginat 
            FROM tbl_users 
            WHERE email = @Email";

        #endregion

        #region User Signature Queries

        public static readonly string INSERT_USER_SIGNATURE_QUERY = @"
            INSERT INTO tbl_user_signatures (userid, signature, createddate,modifiedby)
            VALUES (@UserId, @Signature, GETDATE(),@Modifiedby)";

        public static readonly string GET_USER_SIGNATURE_BY_USERID = @"
            SELECT TOP 1 signature
            FROM tbl_user_signatures
            WHERE userid = @UserId
            ORDER BY createddate DESC";

        public static readonly string GET_USERS_WITH_SIGNATURES = @"
            SELECT
                s.id            AS SignatureId,
                s.userid        AS UserId,
                u.username      AS UserName,
                u.userid        AS EmployeeId,
                d.id            AS DepartmentId,
                d.name          AS DepartmentName,
                s.createddate   AS SignatureCreatedDate,
                r.role          AS RoleName
            FROM tbl_user_signatures s
            INNER JOIN tbl_users     u ON u.id = s.userid
            INNER JOIN tbl_department d ON d.id = u.departmentid
            INNER JOIN tbl_userroles r ON r.id = u.userroleid
            ORDER BY s.createddate DESC";

        #endregion

        #region User Role Queries

        // Insert user role
        public static readonly string INSERT_USER_ROLE_QUERY = @"
            INSERT INTO tbl_userroles (userid, role, createddate, isactive)
            OUTPUT INSERTED.id
            VALUES (@UserId, @Role, @CreatedDate, @IsActive)";

        // Get refresh token
        public static readonly string GET_REFRESH_TOKEN_QUERY = @"
            SELECT * 
            FROM tbl_userrefreshtokens 
            WHERE token = @Token";

        #endregion
    }
}
