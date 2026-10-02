using Common.Domain;
using IdentityPlatform.Authorization.Domain.Roles.Events;
using IdentityPlatform.Identity.Domain.Users.Events;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using static IdentityPlatform.Authorization.Domain.Permissions;

namespace IdentityPlatform.Authorization.Domain.Roles
{
    public sealed class Role : AggregateRoot<Role>
    {
        private readonly List<string> _permissions = new();

        public string Name { get; private set; }
        public string Description { get; private set; }
        public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();



        private Role( string name, string description) 
        {
            Name = name;
            Description = description;
        }
        public Role()
        {
                //for the dbContext
        }

        private Role(string name, string? description, List<string> permissions)
        {
            
            Name = name;
            Description = description;
            if (permissions != null)
            {
                _permissions.AddRange(permissions);
            }
        }

        public static Role Create(string name, string description, List<string> permissions)
        {
            // يمكنك إضافة قواعد التحقق (Validation) هنا
            var role = new Role( name, description, permissions);
            role.RaiseDomainEvent(new RoleCreatedEvent(role.Id, role.Name, role.Description)); 
            return role;
        }

        public void AddPermission(string permission)
        {
            if (!_permissions.Contains(permission))
            {
                _permissions.Add(permission);
                RaiseDomainEvent(new UserPermissionAddedEvent(this.Id , permission));
            }
        }

        public void RemovePermission(string permission)
        {
            _permissions.Remove(permission);
            RaiseDomainEvent(new UserPermissionRemovedEvent(this.Id, permission));
        }

        public void UpdateDetails(string name, string description)
        {
            Name = name;
            Description = description;
            RaiseDomainEvent(new UserPermissionUpdatedEvent(this.Id, name ,description));
        }
    }
}
