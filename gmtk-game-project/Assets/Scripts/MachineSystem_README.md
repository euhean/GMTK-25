# Sistema de Configuración de Máquinas

Este sistema permite crear máquinas dinámicamente usando ScriptableObjects en lugar de prefabs estáticos.

## Componentes del Sistema

### 1. MachineConfiguration (ScriptableObject)
Define la configuración completa de una máquina:
- **Tipo de máquina**: Shapeshifter o Huehopper
- **Propósito**: Qué forma o color produce
- **Configuración visual**: Prefab base, material, sprite, color
- **Datos específicos**: Shape o ResourceColor ScriptableObject
- **Configuración de transform**: Escala y rotación

### 2. MachinePrefabGenerator (Clase estática)
Genera dinámicamente las máquinas basándose en MachineConfiguration:
- Instancia el prefab base
- Aplica la configuración visual
- Configura el componente apropiado (Shapeshifter/Huehopper)
- Aplica transformaciones

### 3. MachineInfo (Actualizada)
Ahora usa MachineConfiguration en lugar de GameObject:
```csharp
public class MachineInfo
{
    public MachineConfiguration machineConfiguration;
    public float angleDegrees;
}
```

## Cómo Usar el Sistema

### Paso 1: Crear MachineConfiguration
1. **Método automático**: Usar el menú del editor
   - `Assets > Create > Machine > Example Shapeshifter Configuration`
   - `Assets > Create > Machine > Example Huehopper Configuration`
   - `Assets > Create > Machine > Complete Machine Set`

2. **Método manual**: 
   - `Assets > Create > Machine > Configuration`
   - Configurar manualmente todos los campos

### Paso 2: Configurar los Campos Requeridos
En el Inspector de MachineConfiguration:

#### Campos Obligatorios:
- **Machine Type**: Shapeshifter o Huehopper
- **Purpose**: Forma o color que produce la máquina
- **Base Prefab**: Prefab base de la máquina (GameObject vacío con Collider)
- **Machine Data**: 
  - Para Shapeshifter: Shape ScriptableObject
  - Para Huehopper: ResourceColor ScriptableObject
- **Icon Sprite**: Sprite que se mostrará como icono

#### Campos Opcionales:
- **Machine Material**: Material personalizado
- **Icon Color**: Color del icono
- **Scale**: Escala de la máquina
- **Rotation Offset**: Rotación adicional

### Paso 3: Usar en EventConfiguration
1. Abrir tu EventConfiguration
2. En la sección "Machine Infos":
   - Asignar la MachineConfiguration creada
   - Configurar el ángulo (angleDegrees)

## Ejemplo de Configuración Completa

### Para una máquina que convierte a círculos:
1. **Machine Type**: Shapeshifter
2. **Purpose**: Circle
3. **Base Prefab**: Prefab con Collider
4. **Machine Data**: Shape ScriptableObject configurado para Circle
5. **Icon Sprite**: Sprite de círculo
6. **Icon Color**: Azul

### Para una máquina que convierte a rojo:
1. **Machine Type**: Huehopper
2. **Purpose**: Red
3. **Base Prefab**: Prefab con Collider
4. **Machine Data**: ResourceColor ScriptableObject configurado para Red
5. **Icon Sprite**: Sprite de color
6. **Icon Color**: Rojo

## Ventajas del Nuevo Sistema

1. **Flexibilidad**: Configuración dinámica sin necesidad de múltiples prefabs
2. **Mantenibilidad**: Cambios centralizados en ScriptableObjects
3. **Escalabilidad**: Fácil agregar nuevos tipos de máquinas
4. **Reutilización**: Un prefab base puede generar múltiples tipos de máquinas
5. **Validación**: Sistema de validación automática de configuraciones

## Migración desde el Sistema Anterior

El sistema anterior usaba `machinePrefab` directamente. Ahora:
- `machinePrefab` → `machineConfiguration`
- Los prefabs existentes pueden usarse como `basePrefab` en las configuraciones
- CintaController automáticamente usa MachinePrefabGenerator

## Troubleshooting

### Error: "Invalid configuration"
- Verificar que Machine Type coincida con Machine Data
- Asegurar que todos los campos obligatorios estén asignados

### Error: "MachineConfiguration is null"
- Verificar que MachineInfo tenga asignada una MachineConfiguration
- Revisar que el ScriptableObject no esté corrupto

### Las máquinas no aparecen
- Verificar que Base Prefab tenga un Collider
- Revisar que la configuración sea válida
- Comprobar los logs de Unity para errores específicos