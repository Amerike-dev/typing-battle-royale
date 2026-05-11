# Cómo crear Players con skins diferentes

Esta guía explica cómo agregar nuevos personajes con variantes de skin al sistema de selección y gameplay.

El sistema utiliza `ScriptableObjects` de tipo `SkinInfo` para separar dos tipos de prefabs:

- **Preview Models:** modelos visuales que aparecen en la escena de selección.
- **Gameplay Prefabs:** prefabs jugables que se instancian en `GameplayScene`.

---

## 1. Importa tu Personaje e Skins Variantes

Cuando vayas a importar nuevos personajes hazlo en la siguiente Ruta:

`Assets/Prefabs/PrefabList/SkinInfo`

Haras una carpeta con el nombre del "Personaje" y dentro haras dos carpetas nuevas. Uno llamado Visual3D y otro llamado "Gameplay". 

Colocalos de forma ordenada y el Gameplay hay que agregar sus respectivos scripts ya que sera el personaje jugable.

---

## 2. Crear un nuevo SkinInfo

En la misma ruta del anterior paso.

Crea un `ScriptableObject` basado en `SkinInfo`. 

---

## 3. Abre el Nuevo SkinInfo

Una vez abierto veras los campos anteriormente mencionado. Rellenalos con su respectivo Modelo y Gameplay Prefabs.

Colocalos de acuerdo a como se veran en juego. Ejemplo:

* `Model`[ **Model_01**, **Model_02**, ... ] = `Gameplay Prefabs`[ **Gameplay_01**, **Gameplay_02**, ... ]

---

## 4. Modificaremos IDController y GameplayManager

En la siguiente ruta `Assets/Prefaps` veras el Prefab de **BasicPrefab** 

Abrelo, contendra el siguiente script **ID Controller** en **Array Skins** agregaras el nuevo Scriptable.

Y lo mismo pero en la escena `GameplayScene` en **GameplayManager** en el script del mismo nombre, en **Array Skins** agregaras el mismo Scriptable.

Asi funciona el sistema de personajes. Toda duda por favor consultar con el Mentor: Banyo. 