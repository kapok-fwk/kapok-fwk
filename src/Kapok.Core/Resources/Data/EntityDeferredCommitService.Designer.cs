﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kapok.Resources.Data {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class EntityDeferredCommitService {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EntityDeferredCommitService() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Kapok.Resources.Data.EntityDeferredCommitService", typeof(EntityDeferredCommitService).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die You try to create an entity of type {0} which already exists. Primary key values: {1} ähnelt.
        /// </summary>
        internal static string CreateExistingEntityError {
            get {
                return ResourceManager.GetString("CreateExistingEntityError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die EntityDeferredCommitService&lt;{0}&gt;: The method {1} can only be called for entity types which have a primary key ähnelt.
        /// </summary>
        internal static string EntityTypeHasNoPrimaryKey {
            get {
                return ResourceManager.GetString("EntityTypeHasNoPrimaryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Inconsistency in primary key cache behind EntityDeferredCommitService: The primary key cache has already tracked the entry of type {0} with primary key {1} which can not be found in the change tracker. Error occured in method {2} ähnelt.
        /// </summary>
        internal static string PrimaryKeyCacheInconsistencyError {
            get {
                return ResourceManager.GetString("PrimaryKeyCacheInconsistencyError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Duplicated key issue: You try start tracking an entity of type {0} with a key which is already used not yet saved to the database. Please modify the entity instance already used or save the changes and then start modifying the saved entity retrieved from the database. ähnelt.
        /// </summary>
        internal static string StartTrackingAlreadyTrackendEntryStateCreated {
            get {
                return ResourceManager.GetString("StartTrackingAlreadyTrackendEntryStateCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die You try to start tracking an entity of type {0} with a different version of an already tracked entity which is changed. Please try reloading the whole context {1} again or reject the changes before calling {2}. ähnelt.
        /// </summary>
        internal static string StartTrackingAlreadyTrackendEntryStateUpdated {
            get {
                return ResourceManager.GetString("StartTrackingAlreadyTrackendEntryStateUpdated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die Inconsistency Error: Tracking object must have property {0} given when calling {1}. Entity Type: {2}, Entity Primary Key: {3} ähnelt.
        /// </summary>
        internal static string TrackUpdateInconsistencyInChangeTrackerObject {
            get {
                return ResourceManager.GetString("TrackUpdateInconsistencyInChangeTrackerObject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die It is not supported to update a not tracked entity. You have to start tracking first by calling method {1}. Entity: {0} ähnelt.
        /// </summary>
        internal static string UpdateNotTrackedEntityError {
            get {
                return ResourceManager.GetString("UpdateNotTrackedEntityError", resourceCulture);
            }
        }
    }
}
