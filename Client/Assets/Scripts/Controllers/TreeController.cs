using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    SpriteRenderer sr;
    List<Collider2D> _list = new List<Collider2D>();

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _list.Add(collision);
            sr.color = new Color(1, 1, 1, 0.3f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _list.Remove(collision);
        }

        if(_list.Count == 0)
            sr.color = new Color(1, 1, 1, 1f);
    }
}
