//-----------------------------------------------------------------
// A class to allow the drawing of multiple "quads" as part of a
// single aggregated mesh so as to achieve multiple, independently
// moving objects using a single draw call.
//-----------------------------------------------------------------

using UnityEngine;


public class GUISprite : System.Object
{
    public GUISpriteManager manager = null;      // Reference to the sprite manager in which this sprite resides
    public bool m_hidden___DoNotAccessExternally = false;   // Indicates whether this sprite is currently hidden (has to be public because C# has no "friend" feature, just don't access directly from outside)

    public float width;  // Width and Height of the sprite in worldspace units. DO NOT SET THESE
    public float height; // THESE ARE PUBLIC TO AVOID THE GETTER OVERHEAD
	public bool gameObjectOriginInCenter = false;  // Set to true to get your origin in the center.  Useful for scaling/rotating
    protected GameObject client;        // Reference to the client GameObject
	protected UVRect _uvFrame;		// UV coordinates and size for the sprite
	
    protected Vector3[] meshVerts;        // Pointer to the array of vertices in the mesh
    protected Vector2[] UVs;              // Pointer to the array of UVs in the mesh

    public Transform clientTransform;         // Cached Transform of the client GameObject
    public Color _color;       // The color to be used by all four vertices

    public int index;                     // Index of this sprite in its SpriteManager's list

    public Vector3 v1 = new Vector3();      // The sprite's vertices in local space
    public Vector3 v2 = new Vector3();
    public Vector3 v3 = new Vector3();
    public Vector3 v4 = new Vector3();

	// Indices of the associated vertices in the actual mesh (this just provides a quicker way for the SpriteManager to get straight to the right vertices in the vertex array)
	// Alsow houses indices of UVs in the mesh and color values
	public GUIVertexIndices vertexIndices;


    public GUISprite( Rect frame, int depth, UVRect uvFrame )
    {
		// Setup our GO
		client = new GameObject( "UIElement" );
		client.transform.parent = GUISpriteUI.instance.transform; // Just for orginization in the hierarchy
		client.layer = GUISpriteUI.instance.layer; // Set the proper layer so we only render on the UI camera
		client.transform.position = new Vector3( frame.x, -frame.y, depth ); // Depth will affect z-index
		
		// Cache the clientTransform
		clientTransform = client.transform;
		
		// Save these for later.  The manager will call initializeSize() when the UV's get setup
		width = frame.width;
		height = frame.height;
		_uvFrame = uvFrame;
    }


	public virtual UVRect uvFrame
	{
		get { return _uvFrame; }
		set
		{
			// Dont bother changing if the new value isn't different
			if( _uvFrame != value )
			{
				_uvFrame = value;
				manager.updateUV( this );
			}
		}
	}


    public bool hidden
    {
        get { return m_hidden___DoNotAccessExternally; }
        set
        {
            // No need to do anything if we're already in this state:
            if( value == m_hidden___DoNotAccessExternally )
                return;

            if( value )
                manager.hideSprite( this );
            else
                manager.showSprite( this );
        }
    }


	// This gets called by the manager just after the UV's get setup
	public void initializeSize()
	{
		setSize( width, height );
		manager.updateUV( this );
	}

	
    // Sets the physical dimensions of the sprite in the XY plane
    public void setSize( float width, float height )
    {
        this.width = width;
        this.height = height;
		
		if( gameObjectOriginInCenter )
		{
			// Some objects need to rotate so we set the origin at the center of the GO
			Vector3 offset = Vector3.zero;
			v1 = offset + new Vector3( -width / 2, height / 2, 0 );   // Upper-left
			v2 = offset + new Vector3( -width / 2, -height / 2, 0 );  // Lower-left
			v3 = offset + new Vector3( width / 2, -height / 2, 0 );   // Lower-right
			v4 = offset + new Vector3( width / 2, height / 2, 0 );    // Upper-right
		}
		else
		{
			// Make the origin the top-left corner of the GO
	        v1 = new Vector3( 0, 0, 0 );   // Upper-left
	        v2 = new Vector3( 0, -height, 0 );  // Lower-left
	        v3 = new Vector3( width, -height, 0 );   // Lower-right
	        v4 = new Vector3( width, 0, 0 );    // Upper-right
		}
		
        transform();
    }
	

    // Sets the vertex and UV buffers
    public void setBuffers( Vector3[] v, Vector2[] uv )
    {
        meshVerts = v;
        UVs = uv;
    }
	

    // Applies the transform of the client GameObject and stores the results in the associated vertices of the overall mesh
    public virtual void transform()
    {
		meshVerts[vertexIndices.mv.one] = clientTransform.TransformPoint( v1 );
		meshVerts[vertexIndices.mv.two] = clientTransform.TransformPoint( v2 );
		meshVerts[vertexIndices.mv.three] = clientTransform.TransformPoint( v3 );
		meshVerts[vertexIndices.mv.four] = clientTransform.TransformPoint( v4 );

        manager.updatePositions();
    }
	

    // Sets the specified color and automatically notifies the GUISpriteManager to update the colors
	public Color color
	{
		get { return _color; }
		set
		{
			_color = value;
			manager.updateColors( this );
		}
	}

}

