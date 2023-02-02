using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> plInterfaces = new List<PlayerInterfaces>();
    public PotraitInfo[] potraitPrefabs;
    public int maxX;
    public int maxY;
    PotraitInfo[,] charGrid;

    public GameObject potraitCanvas;

    bool loadLevel;
    public bool bothPlayerSelected;

    CharacterManager charManager;
     
    public static SelectScreenManager instance;
    public static SelectScreenManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        charManager = CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;

        charGrid = new PotraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        potraitPrefabs = potraitCanvas.GetComponentsInChildren<PotraitInfo>();

        for(int i = 0; i < potraitPrefabs.Length; i++)
        {
            potraitPrefabs[i].posX += x;
            potraitPrefabs[i].posY += y;

            charGrid[x, y] = potraitPrefabs[i];

            if(x < maxX - 1)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }
    }

    void Update()
    {
        if(!loadLevel)
        {
            for(int i = 0; i < plInterfaces.Count; i++)
            {
                if(i < numberOfPlayers)
                {
                    if (Input.GetButtonUp("Fire2" + charManager.players[i].inputId))
                    {
                        plInterfaces[i].playerBase.hasCharacter = false;
                    }

                    if (!charManager.players[i].hasCharacter)
                    {
                        plInterfaces[i].playerBase = charManager.players[i];

                        HandleSelectorPosition(plInterfaces[i]);
                        HandleSelectScreenInput(plInterfaces[i], charManager.players[i].inputId);
                        HandleCharacterPreview(plInterfaces[i]);
                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;
                }
            }
        }

        if (bothPlayerSelected)
        {
            Debug.Log("loading");
            StartCoroutine("LoadLevel");
            loadLevel = true;
        }
        else
        {
            if(charManager.players[0].hasCharacter 
                && charManager.players[1].hasCharacter)
            {
                bothPlayerSelected = true;
            }
        }
    }

    void HandleSelectScreenInput(PlayerInterfaces pl, string playerId)
    {
        float vertical = Input.GetAxis("Vertical" + playerId);

        if(vertical != 0)
        {
            if(!pl.hitInputOnce)
            {
                if(vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxY - 1;
                }
                else
                {
                    pl.activeY = (pl.activeY < maxY - 1) ? pl.activeY + 1 : 0;
                }

                pl.hitInputOnce = true;
            }
        }

        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if (horizontal != 0)
        {
            if (!pl.hitInputOnce)
            {
                if (horizontal > 0)
                {
                    pl.activeX = (pl.activeX > 0) ? pl.activeX - 1 : maxX - 1;
                }
                else
                {
                    pl.activeX = (pl.activeX < maxX - 1) ? pl.activeX + 1 : 0;
                }

                pl.timerToReset = 0;
                pl.hitInputOnce = true;
            }
        }

        if(vertical == 0 && horizontal == 0)
        {
            pl.hitInputOnce = false;
        }

        if(pl.hitInputOnce)
        {
            pl.timerToReset += Time.deltaTime;

            if(pl.timerToReset > 0.8f)
            {
                pl.hitInputOnce = false;
                pl.timerToReset = 0;
            }
        }

        if(Input.GetButtonUp("Fire1" + playerId))
        {
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");

            pl.playerBase.playerPrefab = 
                charManager.returnCharacterWithID(pl.activePotrait.characterId).prefab;

            pl.playerBase.hasCharacter = true;
        }
    }

    IEnumerator LoadLevel()
    {
        for(int i = 0; i < charManager.players.Count; i++)
        {
            if(charManager.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                if(charManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, potraitPrefabs.Length);

                    charManager.players[i].playerPrefab =
                        charManager.returnCharacterWithID(potraitPrefabs[ranValue].characterId).prefab;

                    Debug.Log(potraitPrefabs[ranValue].characterId);
                }
            }
        }

        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("level", LoadSceneMode.Single);
    }

    void HandleSelectorPosition(PlayerInterfaces pl)
    {
        pl.selector.SetActive(true);

        pl.activePotrait = charGrid[pl.activeX, pl.activeY];

        Vector2 selectorPosition = pl.activePotrait.transform.localPosition;

        selectorPosition = selectorPosition + new Vector2(potraitCanvas.transform.localPosition.x
            , potraitCanvas.transform.localPosition.y);

        pl.selector.transform.localPosition = selectorPosition;
    }

    void HandleCharacterPreview(PlayerInterfaces pl)
    {
        if(pl.previewPotrait != pl.activePotrait)
        {
            if(pl.createdCharacter != null)
            {
                Destroy(pl.createdCharacter);
            }

            GameObject go = Instantiate(
                CharacterManager.GetInstance().returnCharacterWithID(pl.activePotrait.characterId).prefab,
                pl.charVisPos.position,
                Quaternion.identity) as GameObject;

            pl.createdCharacter = go;

            pl.previewPotrait = pl.activePotrait;

            if(!string.Equals(pl.playerBase.playerId, charManager.players[0].playerId))
            {
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }

    [System.Serializable]
    public class PlayerInterfaces
    {
        public PotraitInfo activePotrait;
        public PotraitInfo previewPotrait;
        public GameObject selector;
        public Transform charVisPos;
        public GameObject createdCharacter;

        public int activeX;
        public int activeY;

        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;
    }
}
