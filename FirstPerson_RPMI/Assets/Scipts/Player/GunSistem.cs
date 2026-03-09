using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSistem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam;
    [SerializeField] Transform shootPoint;
    [SerializeField] LayerMask impactLayer;
    RaycastHit hit;

    [Header("Weapons Parameters")]
    [SerializeField] int damage = 10;
    [SerializeField] float range = 100f;
    [SerializeField] float spread = 0f;
    [SerializeField] float shootingCooldown = 0.2f;
    [SerializeField] float reloadTime = 1.5f;
    [SerializeField] bool allowButtonHold = false;

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30;
    [SerializeField] int bulletsPerTap = 1;
    [SerializeField]int bulletsLeft;

    [Header("Feedback Reference")]
    [SerializeField] GameObject impactEffect;

    [Header("Dev - Gun State Bools")]
    [SerializeField] bool shooting;
    [SerializeField] bool canShoot;
    [SerializeField] bool reloading;
    #endregion

    void Awake()
    {
        bulletsLeft = ammoSize;
        canShoot = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(canShoot && shooting && !reloading)
        {
            //Inicializar el proceso de disparo
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;//Primera capa de seguridad que evita que apilemos disparos
        if(!allowButtonHold) shooting = false;//Configuración del disparo por tap
        for(int i = 0; i < bulletsPerTap; i++)
        {
            if(bulletsLeft <= 0) break;//Segunda capa de seguridad que evita que disparemos sin balas

            Shoot();//Disparo en sí = Raycast que permite daño
            bulletsLeft--;//Quita una bala del cargador actual
        }

        yield return new WaitForSeconds(shootingCooldown);//Ejecución de la espera entre disparos
        canShoot = true;//Se devuelve la posibilidad de disparar
    }

    void Shoot()
    {
        //Almacenar la dirección de disparo y modificarla en caso 
        Vector3 direction = fpsCam.transform.forward;
        //Añadir dispersión aleatoria según el valor de spread
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);
        //DECLARACIÓN DEL RAYCAST
        //Phisics.Raycast(Origen del rayo, dirección, almacén de la info del impacto, longitud del rayo, layer con la que impacta)
        if (Physics.Raycast(fpsCam.transform.position, direction, out hit, range, impactLayer))
        {
            Debug.Log(hit.collider.name);
            if(hit.collider.CompareTag("Enemy"))
            {
               EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    IEnumerator ReloadRoutine()
    {
        reloading = true;
        //Aquí iría la llamada a la animación de recarga
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = ammoSize;
        reloading = false;
    }

    void Reload()
    {
        if(bulletsLeft < ammoSize && !reloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    #region Input Methods

    public void OnShoot(InputAction.CallbackContext context)
    {
       //El sistema de input debe comprobar si el disparo es por tap o por mantener
       if(allowButtonHold)
       {
            //Modo mantener ON
            shooting = context.ReadValueAsButton();
       }
       else
       {
            //Modo mantener OFF, solo dispara al realizar la acción, no al mantenerla
            if(context.performed) shooting = true;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(context.performed) Reload();
    }
    #endregion
}
