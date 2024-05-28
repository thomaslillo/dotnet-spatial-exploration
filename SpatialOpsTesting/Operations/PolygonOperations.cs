using NetTopologySuite.Operation.Union;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace SpatialOpsTesting.Operations;

/// <summary>
/// Combine multiple geometries into a single geometry 
/// </summary>
public interface IPolygonCombiner
{
    /// <summary>
    /// Combine the specified geometries into a a single geometry and
    /// then convert to WKT.
    /// </summary>
    /// <remarks>
    /// The individual geometries do not have their details "unioned". The initial
    /// exploration of this capability in NetTopologySuite throw exceptions complainng
    /// about odd polygon issues. The ALterYX sample file also threw an exception , but 
    /// a different one. Actual unioning geometries will require deeper GIS and NetTopology 
    /// knowledge (if possible at all).
    /// </remarks>
    /// <param name="allGeometry"></param>
    /// <param name="areatype"></param>
    /// <returns></returns>
    string CombinePolygons(IList<Geometry> allGeometry, AreaType? areatype = AreaType.WKT);
}

///<inheritdoc/>
public class PolygonCombiner : IPolygonCombiner
{
    ///<inheritdoc/>
    public string CombinePolygons(IList<Geometry> polygons, AreaType? areatype = AreaType.WKT)
    {
        var externalConverter = new ExternalFormatConverter();

        var validGeometries = new List<Geometry>();

        //foreach (var geo in polygons)
        //{
        //    // A GeometryCollection may contain ovelapping polygons, which will throw an error if they are
        //    // provided to the CascadedPolygonUnion method, so in these cases we need to dissolve their elements
        //    // into a single polygon before unioning.
        //    if (geo.GeometryType == "GeometryCollection")
        //    {
        //        // Extract the Polygon geometries from the GeometryCollection
        //        var collectionPolys = new List<Polygon>();
        //        foreach (var item in ((GeometryCollection)geo).Geometries)
        //        {
        //            if (item is Polygon polygon)
        //            {
        //                collectionPolys.Add(polygon);
        //            }
        //        }
        //        var dissolvedGeometry = UnaryUnionOp.Union(collectionPolys);
        //        if (dissolvedGeometry != null)
        //        {
        //            validGeometries.Add(dissolvedGeometry);
        //        }
        //    }
        //    else
        //    {
        //        validGeometries.Add(geo);
        //    }
        //}

        //foreach (var geo in polygons)
        //{
        //    if (geo.GeometryType == "Polygon" || geo.GeometryType == "MultiPolygon")
        //    {
        //        validGeometries.Add(geo);
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Polygon of Invalid Type {geo.GeometryType} Not Combined");
        //    }
        //}

        foreach (var geo in polygons)
        {
            if (geo is Polygon polygon)
            {
                validGeometries.Add(polygon);
            }
            else if (geo is MultiPolygon multiPolygon)
            {
                validGeometries.Add(multiPolygon);
            }
            else
            {
                Console.WriteLine($"Polygon of Invalid Type {geo.GeometryType} Not Combined");
            }
        }

        if (validGeometries.Count < 1)
        {
            return string.Empty;
        }

        //// Will only union POLYGONs, and will exclude POINTs and LINESTRINGs,
        //// if all polygons overlap, will create a POLYGON, otherwise will create
        //// a MULTIPOLYGON WKT.
        var unioner = new CascadedPolygonUnion(validGeometries);
        var finalGeo = unioner.Union();

        if (areatype == AreaType.GeoJson)
        {
            return externalConverter.ToGeoJson(finalGeo);
        }

        return externalConverter.ToWKT(finalGeo);
    }
}

public class ExternalFormatConverter
{
    ///<inheritdoc/>
    public string ToWKT(Geometry geometry)
    {
        // Output wkt
        var writer = new WKTWriter();
        var result = writer.Write(geometry);
        return result;
    }

    ///<inheritdoc/>
    public string ToGeoJson(Geometry geometry)
    {
        // Output wkt
        var serializer = GeoJsonSerializer.Create();
        using var stringWriter = new StringWriter();
        using var jsonWriter = new JsonTextWriter(stringWriter);
        serializer.Serialize(jsonWriter, geometry);
        return stringWriter.ToString();
    }

    ///<inheritdoc/>
    public Geometry FromWKT(string wkt)
    {
        var wktReader2 = new WKTReader();
        return wktReader2.Read(wkt);
    }
}

