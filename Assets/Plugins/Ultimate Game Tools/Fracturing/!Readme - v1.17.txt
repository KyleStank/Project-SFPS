________________________________________________________________________________________
                         Ultimate Fracturing & Destruction Tool
                        Copyright © 2014-2016 Ultimate Game Tools
                            http://www.ultimategametools.com
                               info@ultimategametools.com

                                         Twitter (@ugtools): https://twitter.com/ugtools
                                    Facebook: https://www.facebook.com/ultimategametools
                               Google+:https://plus.google.com/u/0/117571468436669332816
                                 Youtube: https://www.youtube.com/user/UltimateGameTools
________________________________________________________________________________________
Version 1.17


________________________________________________________________________________________
Introduction

The Ultimate Fracturing & Destruction Tool is a Unity3D editor extension that allows
you to fracture, slice and explode meshes. It also allows to visually edit behaviors
for all in-game destruction events like collisions, making it possible to trigger sounds
and particles when collisions occur without the need to script anything.

The Ultimate Fracturing & Destruction's main features are:
-Includes different sample scenes.
-Includes full source code.
-BSP (recursive slicing) & Voronoi fracturing algorithms.
-Can also import fractured objects from external tools (RayFire, Blender...).
-Can generate chunk connection graph to enable structural behavior. Destruction will
 behave intelligently depending on how the chunks are interconnected and also connected
 to the world.
-Can detect mesh islands.
-Automatically maps the interior of the fractured mesh. You assign the material.
-Visual editor to handle all events allowing to specify particle systems, sounds or
 any prefabs that need to be instanced when detaching, collisions, bullet impacts or
 explosions occur.
-Full UI integration allowing you to visualize and edit everything in the editor.
-Includes our Mesh Combiner utility to enable compound object fracturing as well.
-If you have our Concave Collider tool, you have additional control over the colliders
 generated.
-Many many paramters to play with!


________________________________________________________________________________________
Requirements

Supports Unity 3.5+, 4.x and 5


________________________________________________________________________________________
Help

For up to date help: http://www.ultimategametools.com/products/fracturing/help
For additional support contact us at http://www.ultimategametools.com/contact


________________________________________________________________________________________
Acknowledgements

-3D Models:
 Temple model by xadmax2: http://www.turbosquid.com/FullPreview/Index.cfm/ID/548946
 Gun model by psionic: http://www.psionic3d.co.uk/?page_id=25

-The fracturing algorithm uses Poly2Tri for Delaunay triangulation on mesh capping:
 http://code.google.com/p/poly2tri/


________________________________________________________________________________________
Version history

V1.17 - 04/02/2016:

[FIX] - Fixed compiler deprecated warnings for newer versions of Unity 5.

V1.16 - 21/09/2015:

[FIX] - Got rid of the following error: `UnityEngine.Material.Material(string)' is
        obsolete: `Creating materials from shader source string will be removed in the
        future. Use Shader assets instead.'"
        Particularly in Unity 5.2 it showed nasty pink polygon glitches in the editor.
[FIX] - Vertex colors should work OK now.
[FIX] - Concave Collider should always be properly detected now.

V1.15 - 27/06/2015:

[ADD] Added new PlayMaker actions.

V1.14 - 15/05/2015:

[FIX] Removed the following error got on some meshes:
	  "Failed getting triangles. Submesh i has no indices."

V1.13 - 04/05/2015:

[FIX] Fixed Concave Collider compatibility.

V1.12 - 04/03/2015:

[NEW] Added full support for Unity 5.

V1.11 - 07/12/2013:

[FIX] In newer (4.3) versions of Unity when prompted "Do you want to hide the original
      object and place the new fractured object in its position", if Yes was selected
      the single object created (@name) was initially inactive. This caused the initial
      state of the object to be invisible. This is now fixed.
[FIX] If the Source Object is a prefab or an object not part of the scene, the user
      is never prompted "Do you want to hide the original object and place the new
      fractured object in its position" when computing chunks anymore.
[FIX] Fixed collider errors ("ConvexMesh::loadConvexHull: convex hull init failed!")
      in scene #4
[FIX] Fixed rutime errors when using the non-documented w key to throw spheres in
      scene #4.
[FIX] Fixed deprecated warnings regarding Undo operations in Unity 4.3+.
[FIX] Fixed empty meshfilters being generated for the single object (@name) when
      fracturing a procedurally generated object. ProBuilder and other packages generate
      meshes procedurally.

Special thanks to Marc Fraley for helping out with the ProBuilder support fixes!

V1.10 - 15/11/2013:

[NEW] Now when computing chunks an additional single object is created and is used to
      render the fractured object when no chunk has been detached. This decreases the
      number of drawcalls dramatically, especially when multiple fractured objects are
      used in the same scenario.
      The object can be identified because it has a @ preceding its name and will be
      child of the fractured object.
[NEW] Now you can select the convex decomposition algorithm of the Concave Collider
      plugin.
      Note: Some advanced parameters can be uncovered on the GUI by editing the file
      FracturedObjectEditor.cs (search for "Uncomment for advanced control" string).
[NEW] Now chunks can be reseted to their original position (as long as none has been
      deleted). This can be really useful to reuse the same prefab instead of multiple
      instances. Especially to avoid instancing on mobile devices.
[NEW] Now support chunks can optionally be destructible as well (new parameter
      "Support Is Indestructible".
[NEW] Now when unchecking and checking again the "Enable Prefab Usage" (old name
      "Save Mesh Data To Asset") you force the select destination asset file dialog
      again. This is useful in case you copy a gameobject but want to assign it a
      different asset file.
[NEW] Now the fracturing process can also be cancelled during the asset file creation.
[NEW] Now you can get events every time a chunk is detached (last section of the Events
      parameters). Previously only detach events due to physics collision could be
      handled. Note that if both are enabled you will get one event to each handler.
[FIX] Concave Collider's fast algorithm now falls back to normal if it fails.
[FIX] Concave Collider generated hulls now inherit their chunk layer.
[FIX] Chunks now get assigned the same layer as the Source Object instead of the
      fractured object itself. This can solve potential visibility/lighting/collision
      filtering issues.
[CHG] "Save Mesh Data To Asset" parameter has been renamed to "Enable Prefab Usage" for
      better understanding.
[CHG] "Compute Colliders" now can't be used if "Enable Prefab Usage" is enabled.
      Previously this gave an unreferenced exception.
[CHG] BSP fracturing now has more precision. Computations are performed centered in
      (0, 0, 0) where there is more floating point precision. Previously if the source
      object was far away there could be potential glitches and missing polygons.

V1.04 - 14/09/2013:

[FIX] Now support planes are correctly saved to disk when "Save Mesh Data To Asset" is
      enabled. Before, a NullReferenceException was thrown from the method
      UnityEngine.Graphics.Internal_DrawMeshNow2.
[FIX] Now prefab destructible objects don't spawn at (0, 0, 0).
[FIX] Got rid of the gameObject.active obsolete warning on Unity 4.x

V1.03 - 24/06/2013:

[NEW] Now can detect individual chunks when a Combined Mesh is the input. This allows
      to import objects from external fracturing tools like RayFire.
      Chunk connection graph is also generated in this case.
[DEL] Removed FracturedObject.EnableRandomColoredChunks().

V1.02 - 16/06/2013:

[FIX] Changed FracturedObject.cs GUID because it collided with UltimateRope.cs from our
      Ultimate Rope Editor package. It caused erroneous imports when both packages
      were added to the same project.
[DEL] Removed an empty Fractured Object in the first scene.

V1.00 - 29/05/2013:

[---] Initial release