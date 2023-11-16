﻿using BlockEngine.Framework.Registries;

namespace BlockEngine.Framework.Blocks;

public class Blocks
{
    public static readonly Block Air = IdRegistry.Blocks.Register("be:air", new Block(0, BlockVisibility.Empty));
    public static readonly Block Stone = IdRegistry.Blocks.Register("be:stone", new Block(1, BlockVisibility.Opaque));
}