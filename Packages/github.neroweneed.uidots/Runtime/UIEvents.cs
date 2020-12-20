using Unity.Entities;

namespace NeroWeNeed.UIDots {
public unsafe delegate void UISelectDelegate(Entity* cursor,Entity* element,void* ecb);
}