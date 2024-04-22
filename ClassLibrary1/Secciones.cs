using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace RevitPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Secciones : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //Esto nos permite trabajar con el proyecto de la vista activa, pq si  tenemo smmuchos abiertos ...
            Document doc = uidoc.Document;

            using (System.Windows.Forms.Form form = new SectionForms())
            {
                if(form.ShowDialog() == DialogResult.OK)
                {
                    //PSeleccionar elementos y en caso que no se seleccione ninguno salir
                    List<Reference> pickedObj = (List<Reference>)uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
                    if (pickedObj == null)
                    {
                        return Result.Failed;
                    }

                    //Ciclo para repetir sobre todos los seleccionados
                    foreach (Reference xs in pickedObj)
                    {
                        ElementId eleId = xs.ElementId;
                        Element ele = doc.GetElement(eleId);

                        //Por si se selecciona otro tipo de elemento, para q lo ignore
                        if (!(ele is DetailLine || ele is Wall))
                        {
                            continue;
                        }

                        ViewFamilyType vft
                      = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .FirstOrDefault<ViewFamilyType>(x =>
                        ViewFamily.Section == x.ViewFamily);

                        //Creo un location curve
                        LocationCurve linea = ele.Location as LocationCurve;

                        //Saco la longitud de la linea
                        double length = linea.Curve.Length;

                        //saco los dos puntos de coordenadas de la linea
                        XYZ p = linea.Curve.GetEndPoint(0);
                        XYZ q = linea.Curve.GetEndPoint(1);
                        XYZ v = q - p;

                        //Creo instancia para convertir unidades
                        //Convierto unidades
                        Conversiones Convertir = new Conversiones();
                        double altura = Configuracion.altura;
                        altura = Convertir.MmToFoot(altura);

                        double desfase = Configuracion.desfase;
                        desfase = Convertir.MmToFoot(desfase);

                        double profundidad = Configuracion.profundidad;
                        profundidad = Convertir.MmToFoot(profundidad);

                        double w = length / 2;
                        XYZ max = new XYZ(w, altura, profundidad);
                        XYZ min = new XYZ(-w, desfase, 0);

                        XYZ midpoint = p + 0.5 * v;
                        XYZ dir = v.Normalize();
                        XYZ up = XYZ.BasisZ;
                        XYZ viewdir = dir.CrossProduct(up);
                        Transform t = Transform.Identity;
                        t.Origin = midpoint;
                        t.BasisX = dir;
                        t.BasisY = up;
                        t.BasisZ = viewdir;


                        BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
                        sectionBox.Transform = t;
                        sectionBox.Min = min;
                        sectionBox.Max = max;

                        try
                        {
                            using (Transaction tx = new Transaction(doc))
                            {
                                tx.Start("Crear Secciones");

                                ViewSection.CreateSection(doc, vft.Id, sectionBox);

                                tx.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Error: " + ex);
                            throw ex;
                        }


                        if (Configuracion.eliminarlineas)
                        {
                            using (Transaction tx = new Transaction(doc))
                            {
                                tx.Start("Borrar Lineas");
                                doc.Delete(ele.Id);
                                tx.Commit();
                            }
                        }
                        else
                        {
                        }

                    }
                    return Result.Succeeded;
                }
                else
                {
                    return Result.Failed;
                }
            }
        }
    }
}