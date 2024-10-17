namespace KeystoneLibrary.Models.DataModels
{
    public abstract class Entity
    {
        public virtual void OnBeforeInsert(string userId = null) { }
        public virtual void OnBeforeUpdate(string userId = null) { }
    }
}
